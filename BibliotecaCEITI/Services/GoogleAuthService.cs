using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

namespace BibliotecaCEITI
{
    public class GoogleAuthResult
    {
        public bool Success { get; init; }
        public string? GoogleId { get; init; }
        public string? Email { get; init; }
        public string? NumeFull { get; init; }
        public string? ErrorMsg { get; init; }
    }

    public class GoogleAuthService
    {
        private static readonly Lazy<GoogleAuthService> _inst =
            new(() => new GoogleAuthService());
        public static GoogleAuthService Instance => _inst.Value;

        private static readonly string TokenStorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BibliotecaLogin", "GoogleTokens");

        private const string RevokeEndpoint = "https://oauth2.googleapis.com/revoke";
        private static readonly string[] Scopes = { "openid", "email", "profile" };

        // set by LoginWindow from the configurare_oauth table before LoginAsync
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        private UserCredential? _cred;

        private GoogleAuthService() { }

        public async Task<GoogleAuthResult> LoginAsync(CancellationToken ct = default)
        {
            await CleanLocalAsync();

            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = ClientId,
                        ClientSecret = ClientSecret
                    },
                    Scopes = Scopes,
                    DataStore = new FileDataStore(TokenStorePath, true),

                    // select_account forces the account picker every time. Deleting the
                    // token folder is not enough, the browser session cookie survives it.
                    UserDefinedQueryParams = new[]
                    {
                        new KeyValuePair<string, string>("prompt", "select_account")
                    }
                });

            UserCredential cred;
            try
            {
                cred = await new AuthorizationCodeInstalledApp(
                        flow, new LocalServerCodeReceiver())
                    .AuthorizeAsync("user", ct);
            }
            catch (OperationCanceledException)
            {
                return Fail("Autentificarea a fost anulată.");
            }
            catch (Exception ex)
            {
                return Fail($"Eroare OAuth: {ex.Message}");
            }

            if (cred.Token?.IdToken is null)
                return Fail("ID Token lipsește din răspunsul Google.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    cred.Token.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { ClientId }
                    });
            }
            catch (Exception ex)
            {
                await RevokeAndCleanAsync(cred);
                return Fail($"Token ID invalid: {ex.Message}");
            }

            _cred = cred;

            return new GoogleAuthResult
            {
                Success = true,
                GoogleId = payload.Subject,
                Email = payload.Email,
                NumeFull = payload.Name
            };
        }

        public async Task LogoutAsync()
        {
            if (_cred is not null)
            {
                await RevokeAndCleanAsync(_cred);
                _cred = null;
            }
            else
            {
                // may still be files left over from a previous crash
                await CleanLocalAsync();
            }
        }

        private async Task RevokeAndCleanAsync(UserCredential cred)
        {
            string? token = cred.Token?.AccessToken ?? cred.Token?.RefreshToken;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                    await http.PostAsync(
                        $"{RevokeEndpoint}?token={Uri.EscapeDataString(token)}", null);
                }
                catch
                {
                    // offline or already expired, the local cleanup below still runs
                }
            }

            try { await cred.RevokeTokenAsync(CancellationToken.None); } catch { }

            await CleanLocalAsync();
        }

        private Task CleanLocalAsync() => Task.Run(() =>
        {
            try
            {
                if (Directory.Exists(TokenStorePath))
                    Directory.Delete(TokenStorePath, recursive: true);
            }
            catch { }
        });

        private static GoogleAuthResult Fail(string msg) =>
            new() { Success = false, ErrorMsg = msg };
    }
}

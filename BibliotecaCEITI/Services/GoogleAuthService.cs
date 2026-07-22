using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.Auth;

namespace BibliotecaCEITI
{
    public class GoogleAuthResult
    {
        public bool Success { get; init; }
        public string? GoogleId { get; init; }   // câmpul `sub` din ID token
        public string? Email { get; init; }
        public string? NumeFull { get; init; }
        public string? ErrorMsg { get; init; }
    }

    public class GoogleAuthService
    {
        // ── Singleton ──────────────────────────────────────────────────────
        private static readonly Lazy<GoogleAuthService> _inst =
            new(() => new GoogleAuthService());
        public static GoogleAuthService Instance => _inst.Value;

        // ── Directorul unde FileDataStore salvează token-urile pe disk ─────
        private static readonly string TokenStorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BibliotecaLogin", "GoogleTokens");

        private const string RevokeEndpoint = "https://oauth2.googleapis.com/revoke";
        private static readonly string[] Scopes = { "openid", "email", "profile" };

        // ── Client ID/Secret — setate din LoginWindow înainte de LoginAsync ─
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // ── Credential activ al sesiunii curente (necesar pentru logout) ────
        private UserCredential? _cred;

        private GoogleAuthService() { }

        // ───────────────────────────────────────────────────────────────────
        //  LOGIN
        //  Garantează că utilizatorul TREBUIE să aleagă contul Google
        //  la fiecare apel, fără auto-login.
        // ───────────────────────────────────────────────────────────────────
        public async Task<GoogleAuthResult> LoginAsync(CancellationToken ct = default)
        {
            // Pasul 1: Curăță orice token local rămas din sesiuni anterioare
            await CleanLocalAsync();

            // Pasul 2: Construiește flow-ul OAuth cu parametrii de securitate
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

                    // ── CHEILE DE SECURITATE ────────────────────────────────
                    // prompt=select_account → Google afișează MEREU selectorul
                    //   de cont, chiar dacă există o sesiune activă în browser.
                    //   Aceasta rezolvă problema cookie-urilor de browser pe
                    //   care Directory.Delete NU le poate atinge.
                    //
                    // access_type=online → NU se emite refresh_token persistent.
                    //   Token-ul expiră în 1 oră și nu poate fi reutilizat
                    //   la repornirea aplicației.
                    // ───────────────────────────────────────────────────────
                    UserDefinedQueryParams = new[]
                    {
                        new KeyValuePair<string, string>("prompt", "select_account")
                    }
                });

            // Pasul 3: Deschide browserul și așteaptă codul de autorizare
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

            // Pasul 4: Validează și parsează ID token-ul JWT
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
                GoogleId = payload.Subject,   // `sub` — ID stabil Google
                Email = payload.Email,
                NumeFull = payload.Name
            };
        }

        // ───────────────────────────────────────────────────────────────────
        //  LOGOUT
        //  Apelat: (1) imediat după login reușit în LoginWindow
        //          (2) la butonul Deconectare din MainWindow
        //          (3) la închiderea aplicației (Window.Closing)
        // ───────────────────────────────────────────────────────────────────
        public async Task LogoutAsync()
        {
            if (_cred is not null)
            {
                await RevokeAndCleanAsync(_cred);
                _cred = null;
            }
            else
            {
                // Curăță oricum — poate rămăseseră fișiere de la un crash anterior
                await CleanLocalAsync();
            }
        }

        // ───────────────────────────────────────────────────────────────────
        //  Helper: revocă la server Google + șterge local
        // ───────────────────────────────────────────────────────────────────
        private async Task RevokeAndCleanAsync(UserCredential cred)
        {
            // Revocăm access_token (sau refresh_token dacă access_token lipsește)
            string? token = cred.Token?.AccessToken ?? cred.Token?.RefreshToken;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                    // POST https://oauth2.googleapis.com/revoke?token=TOKEN
                    await http.PostAsync(
                        $"{RevokeEndpoint}?token={Uri.EscapeDataString(token)}", null);
                }
                catch
                {
                    // Offline sau token deja expirat — continuăm cu ștergerea locală
                }
            }

            // Revocăm și prin SDK (șterge din DataStore intern)
            try { await cred.RevokeTokenAsync(CancellationToken.None); } catch { }

            // Ștergem directorul complet de pe disk
            await CleanLocalAsync();
        }

        // ───────────────────────────────────────────────────────────────────
        //  Helper: șterge directorul FileDataStore de pe disk
        //
        //  DE CE nu este suficient doar Directory.Delete?
        //  → Acesta șterge doar token-urile noastre locale (JSON cu access/refresh token).
        //  → Cookie-urile sesiunii Google din browserul Windows (IE/Edge cache)
        //    sunt stocate SEPARAT și NU sunt atinse de Directory.Delete.
        //  → De aceea prompt=select_account este ESENȚIAL: forțează Google
        //    să ignore cookie-urile de sesiune din browser.
        // ───────────────────────────────────────────────────────────────────
        private Task CleanLocalAsync() => Task.Run(() =>
        {
            try
            {
                if (Directory.Exists(TokenStorePath))
                    Directory.Delete(TokenStorePath, recursive: true);
            }
            catch { /* nu blocăm aplicația dacă ștergerea eșuează */ }
        });

        private static GoogleAuthResult Fail(string msg) =>
            new() { Success = false, ErrorMsg = msg };
    }
}
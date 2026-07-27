using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    public partial class BookAi : UserControl
    {
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        private const string ModelId = "gemini-3.1-flash-lite";
        private static readonly HttpClient client = new HttpClient();

        private readonly List<object> _conversationHistory = new List<object>();

        public ObservableCollection<UiMessage> ChatMessages { get; set; } = new ObservableCollection<UiMessage>();

        private const int CurrentLibrarianId = 1;

        public BookAi()
        {
            InitializeComponent();

            System.Net.ServicePointManager.Expect100Continue = false;

            ChatItemsControl.ItemsSource = ChatMessages;

            AddMessageToUi("Salut! Sunt asistentul tău AI. Cu ce te pot ajuta astăzi?", isUser: false);
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (input.Text == (string)Application.Current.FindResource("Placeholder_AskYourQuestion"))
            {
                input.Text = "";
            }
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            string userPrompt = input.Text.Trim();

            if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt == (string)Application.Current.FindResource("Placeholder_AskYourQuestion"))
            {
                return;
            }

            if (string.IsNullOrEmpty(ApiKey))
            {
                AddMessageToUi("Eroare: Cheia API nu a fost găsită. Setați variabila de mediu 'GEMINI_API_KEY'.", isUser: false);
                return;
            }

            try
            {
                AddMessageToUi(userPrompt, isUser: true);
                input.Text = "";

                var loadingBubble = AddMessageToUi("Se gândește...", isUser: false);

                string rawJson = await GetGeminiRawResponse(userPrompt);

                ChatMessages.Remove(loadingBubble);

                await ProcessAiResponse(rawJson);
            }
            catch (Exception ex)
            {
                AddMessageToUi($"System Error: {ex.Message}", isUser: false);
            }
        }

        private UiMessage AddMessageToUi(string text, bool isUser)
        {
            var message = new UiMessage(text, isUser);
            ChatMessages.Add(message);

            // scroll after the item template has been rendered
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToBottom();
            }), System.Windows.Threading.DispatcherPriority.Background);

            return message;
        }

        private string LoadSystemInstructions()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "Instructions.txt");

                if (File.Exists(path))
                {
                    return File.ReadAllText(path, Encoding.UTF8);
                }

                // when running under the debugger the file sits in the project folder
                string debugFallbackPath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Instructions.txt"));
                if (File.Exists(debugFallbackPath))
                {
                    return File.ReadAllText(debugFallbackPath, Encoding.UTF8);
                }

                System.Diagnostics.Debug.WriteLine("Instructions.txt was not found in execution or source paths.");
                return "Ești asistentul Bibliotecii CEITI. Ajută succint, fără Markdown.";
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Security/Access error: {ex.Message}");
                return "Ești asistentul Bibliotecii CEITI. Ajută succint, fără Markdown.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading instructions file: {ex.Message}");
                return "Ești asistentul Bibliotecii CEITI. Ajută succint, fără Markdown.";
            }
        }

        private async Task<string> GetGeminiRawResponse(string promptText)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={ApiKey}";

            _conversationHistory.Add(new { role = "user", parts = new[] { new { text = promptText } } });

            string dynamicSystemInstruction = LoadSystemInstructions();

            var requestBody = new
            {
                contents = _conversationHistory.ToArray(),
                systemInstruction = new { parts = new[] { new { text = dynamicSystemInstruction } } },
                tools = new[]
                {
                    new {
                        functionDeclarations = new[]
                        {
                            new {
                                name = "InsertBookIntoDb",
                                description = "Inserează automat o carte nouă în baza de date a bibliotecii prin procedura stocată.",
                                parameters = new {
                                    type = "object",
                                    properties = new {
                                        titlu = new { type = "string", description = "Titlul cărții" },
                                        autor = new { type = "string", description = "Numele complet al autorului" },
                                        categorie = new { type = "string", description = "Denumirea categoriei (ex: Programare, Roman, Matematică)" },
                                        descriere = new { type = "string", description = "Scurtă descriere sau rezumat al cărții" },
                                        isbn = new { type = "string", description = "Codul internațional standard al cărții (ISBN)" },
                                        editura = new { type = "string", description = "Numele editurii" },
                                        anPublicare = new { type = "integer", description = "Anul în care a fost publicată cartea (ex: 2024)" },
                                        limba = new { type = "string", description = "Limba în care este scrisă (ex: Română, Engleză, Rusă)" },
                                        pretVanzare = new { type = "number", description = "Prețul cărții în MDL. Default: 0" },
                                        pretChirie = new { type = "number", description = "Prețul chiriei per lună în MDL (valabil doar pentru manuale). Default: 0" }
                                    },
                                    // isbn is required so the model asks for it instead of inserting without one
                                    required = new[] { "titlu", "autor", "categorie", "anPublicare", "limba", "isbn" }
                                }
                            }
                        }
                    }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpResponse = await client.PostAsync(url, content);
            return await httpResponse.Content.ReadAsStringAsync();
        }

        private async Task ProcessAiResponse(string rawJson)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            var part = root.GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0];

            if (part.TryGetProperty("functionCall", out JsonElement functionCall))
            {
                string functionName = functionCall.GetProperty("name").GetString();
                JsonElement args = functionCall.GetProperty("args");

                if (functionName == "InsertBookIntoDb")
                {
                    string titlu = args.GetProperty("titlu").GetString();
                    string autor = args.GetProperty("autor").GetString();
                    string categorie = args.GetProperty("categorie").GetString();
                    string descriere = args.TryGetProperty("descriere", out var d) ? d.GetString() : null;
                    string isbn = args.TryGetProperty("isbn", out var i) ? i.GetString() : null;
                    string editura = args.TryGetProperty("editura", out var e) ? e.GetString() : null;
                    int anPublicare = args.GetProperty("anPublicare").GetInt32();
                    string limba = args.GetProperty("limba").GetString();
                    double pretVanzare = args.TryGetProperty("pretVanzare", out var pv) ? pv.GetDouble() : 0.0;
                    double pretChirie = args.TryGetProperty("pretChirie", out var pc) ? pc.GetDouble() : 0.0;

                    var dbResult = await AddBookAsync(titlu, autor, categorie, descriere, isbn, editura, anPublicare, limba, pretVanzare, pretChirie, null, CurrentLibrarianId);

                    AddMessageToUi($"[Database Code {dbResult.Cod}]: {dbResult.Mesaj}", isUser: false);

                    // feed the result back so the model knows the insert already happened
                    _conversationHistory.Add(new { role = "model", parts = new[] { new { text = $"Sistem: Executat cu succes. Mesaj DB: {dbResult.Mesaj}" } } });
                }
            }
            else if (part.TryGetProperty("text", out JsonElement textProp))
            {
                string aiText = textProp.GetString();

                _conversationHistory.Add(new { role = "model", parts = new[] { new { text = aiText } } });

                // the chat bubbles are plain TextBlocks, so strip the markdown the model still emits
                string cleanResult = aiText
                    .Replace("**", "")
                    .Replace("__", "")
                    .Replace("###", "")
                    .Replace("##", "")
                    .Replace("#", "")
                    .Replace("`", "")
                    .Replace("---", "")
                    .Replace("* ", "• ")
                    .Replace("- ", "• ")
                    .Trim();

                AddMessageToUi(cleanResult, isUser: false);
            }
        }

        private async Task<(int Cod, string Mesaj)> AddBookAsync(string titlu, string autor, string categorie, string descriere, string isbn, string editura, int anPublicare, string limba, double pretVanzare, double pretChirie, byte[] copertaBytes, int idBibliotecar)
        {
            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            using (var cmd = new MySqlCommand("sp_insert_carte", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@p_titlu", titlu);
                cmd.Parameters.AddWithValue("@p_autor", autor);
                cmd.Parameters.AddWithValue("@p_categorie", categorie);
                cmd.Parameters.AddWithValue("@p_descriere", (object)descriere ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_isbn", (object)isbn ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_editura", (object)editura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_an_publicare", anPublicare);
                cmd.Parameters.AddWithValue("@p_limba", limba);
                cmd.Parameters.AddWithValue("@p_pret_mdl", pretVanzare);
                cmd.Parameters.AddWithValue("@p_pret_chirie_mdl", pretChirie);

                var pBlob = cmd.Parameters.Add("@p_coperta", MySqlDbType.LongBlob);
                pBlob.Value = (object)copertaBytes ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@p_creat_de", idBibliotecar);

                var pIdCarteNou = cmd.Parameters.Add("@p_id_carte_nou", MySqlDbType.UInt32);
                pIdCarteNou.Direction = System.Data.ParameterDirection.Output;

                var pCod = cmd.Parameters.Add("@p_cod", MySqlDbType.Int32);
                pCod.Direction = System.Data.ParameterDirection.Output;

                var pMsg = cmd.Parameters.Add("@p_mesaj", MySqlDbType.VarChar, 255);
                pMsg.Direction = System.Data.ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                int codRezultat = pCod.Value != DBNull.Value ? Convert.ToInt32(pCod.Value) : -1;
                string mesajRezultat = pMsg.Value != DBNull.Value ? pMsg.Value.ToString() : "Procedura nu a returnat un mesaj.";

                return (codRezultat, mesajRezultat);
            }
        }
    }

    public class UiMessage
    {
        public string MessageText { get; set; }
        public HorizontalAlignment Alignment { get; set; }
        public CornerRadius BubbleRadius { get; set; }
        public Brush BackgroundBrush { get; set; }
        public Brush TextBrush { get; set; }
        public Thickness BorderThickness { get; set; }
        public Brush BorderBrush { get; set; }

        public UiMessage(string text, bool isUser)
        {
            MessageText = text;

            if (isUser)
            {
                Alignment = HorizontalAlignment.Right;
                BubbleRadius = new CornerRadius(14, 14, 2, 14);

                BackgroundBrush = Application.Current.Resources["PrimaryBlue"] as Brush ?? new SolidColorBrush(Color.FromRgb(37, 99, 235));
                TextBrush = new SolidColorBrush(Colors.White);
                BorderThickness = new Thickness(0);
                BorderBrush = Brushes.Transparent;
            }
            else
            {
                Alignment = HorizontalAlignment.Left;
                BubbleRadius = new CornerRadius(14, 14, 14, 2);

                BackgroundBrush = Application.Current.Resources["InputBackground"] as Brush ?? new SolidColorBrush(Color.FromRgb(243, 244, 246));
                TextBrush = Application.Current.Resources["TextPrimary"] as Brush ?? new SolidColorBrush(Color.FromRgb(17, 24, 39));
                BorderThickness = new Thickness(1);
                BorderBrush = Application.Current.Resources["BorderBrushLight"] as Brush ?? new SolidColorBrush(Color.FromRgb(229, 231, 235));
            }
        }
    }
}

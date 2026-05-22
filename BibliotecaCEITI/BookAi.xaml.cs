using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    public partial class BookAi : UserControl
    {
        // Get the API Key from system environment variables for security
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        private const string ModelId = "gemini-3.1-flash-lite";
        private static readonly HttpClient client = new HttpClient();

        // Maintains the conversation logs so the AI can track historical context between message turns
        private readonly List<object> _conversationHistory = new List<object>();

        // Observable collection bound to the view's ItemsControl for dynamic chat bubbles
        public ObservableCollection<UiMessage> ChatMessages { get; set; } = new ObservableCollection<UiMessage>();

        // ID of the currently authenticated librarian (Can be updated dynamically based on active user context)
        private const int CurrentLibrarianId = 1;

        public BookAi()
        {
            InitializeComponent();

            // Disable specific network wait behavior to optimize API response latency
            System.Net.ServicePointManager.Expect100Continue = false;

            // Bind the data collection to the ItemsControl UI container
            ChatItemsControl.ItemsSource = ChatMessages;

            // Initial friendly greetings message from the AI on component startup
            AddMessageToUi("Salut! Sunt asistentul tău AI. Cu ce te pot ajuta astăzi?", isUser: false);
        }

        // Clear the placeholder text when the user clicks inside the input textbox element
        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (input.Text == "Scrie întrebarea ta...")
            {
                input.Text = "";
            }
        }

        // Main logic triggered when clicking the UI Send button
        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            string userPrompt = input.Text.Trim();

            // Validation: Abort processing if input text payload contains empty spaces or the initial placeholder
            if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt == "Scrie întrebarea ta...")
            {
                return;
            }

            // Enforce checking if API access token configurations are populated
            if (string.IsNullOrEmpty(ApiKey))
            {
                AddMessageToUi("Eroare: Cheia API nu a fost găsită. Setați variabila de mediu 'GEMINI_API_KEY'.", isUser: false);
                return;
            }

            try
            {
                // Render user message to bubble thread and clear input line field
                AddMessageToUi(userPrompt, isUser: true);
                input.Text = "";

                // Display asynchronous visual status feedback object
                var loadingBubble = AddMessageToUi("Se gândește...", isUser: false);

                // 1. Submit the text sequence data and receive the raw structured JSON back from Gemini
                string rawJson = await GetGeminiRawResponse(userPrompt);

                // Remove temporary thinking status indicator before updating finalized data payload turns
                ChatMessages.Remove(loadingBubble);

                // 2. Parse out execution instructions or plain text content from the response payload
                await ProcessAiResponse(rawJson);
            }
            catch (Exception ex)
            {
                AddMessageToUi($"System Error: {ex.Message}", isUser: false);
            }
        }

        // Helper framework wrapper handling UI state bindings and autoscrolling threads securely
        private UiMessage AddMessageToUi(string text, bool isUser)
        {
            var message = new UiMessage(text, isUser);
            ChatMessages.Add(message);

            // Execute asynchronous UI updates across proper rendering passes
            Dispatcher.BeginInvoke(new Action(() => {
                ChatScrollViewer.ScrollToBottom();
            }), System.Windows.Threading.DispatcherPriority.Background);

            return message;
        }

        // Helper method to load system instructions safely from the specific static file path
        private string LoadSystemInstructions()
        {
            try
            {
                // Get the direct path where the .exe file is currently running
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "Instructions.txt");

                if (File.Exists(path))
                {
                    return File.ReadAllText(path, Encoding.UTF8);
                }

                // Alternative fallback to look into the source project folder during active debug sessions
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

        // Handles building parameters and dispatching the HTTP POST payload request targeting Gemini endpoint
        private async Task<string> GetGeminiRawResponse(string promptText)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={ApiKey}";

            // Append the new incoming user input block into our local conversation history container
            _conversationHistory.Add(new { role = "user", parts = new[] { new { text = promptText } } });

            // Extract the instruction payload blocks dynamically from your localized text asset
            string dynamicSystemInstruction = LoadSystemInstructions();

            // Constructing the nested JSON schema representation exactly as Google's AI API infrastructure expects it
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
                                    // Added 'isbn' to the required array so the AI is forced to ask for it before triggering the function
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

        // Evaluates whether the generated response is an automated function intent call or conversational text
        private async Task ProcessAiResponse(string rawJson)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            var part = root.GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0];

            // Case 1: AI evaluated that context parameters match and requests the application to execute a data modification function
            if (part.TryGetProperty("functionCall", out JsonElement functionCall))
            {
                string functionName = functionCall.GetProperty("name").GetString();
                JsonElement args = functionCall.GetProperty("args");

                if (functionName == "InsertBookIntoDb")
                {
                    // Safe property retrieval containing type bindings and default fallback mechanisms 
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

                    // Trigger the native C# ADO.NET wrapper invoking the stored procedure inside MySQL engine
                    var dbResult = await AddBookAsync(titlu, autor, categorie, descriere, isbn, editura, anPublicare, limba, pretVanzare, pretChirie, null, CurrentLibrarianId);

                    // Output custom application messages returned from the database out execution block (p_mesaj)
                    AddMessageToUi($"[Database Code {dbResult.Cod}]: {dbResult.Mesaj}", isUser: false);

                    // Inject a confirmation flag back to our tracker arrays so the engine handles conversational updates properly
                    _conversationHistory.Add(new { role = "model", parts = new[] { new { text = $"Sistem: Executat cu succes. Mesaj DB: {dbResult.Mesaj}" } } });
                }
            }
            // Case 2: Provided input data is missing required details; model handles conversational follow-up questions
            else if (part.TryGetProperty("text", out JsonElement textProp))
            {
                string aiText = textProp.GetString();

                // Store assistant conversational text within the tracker collections to keep context alive
                _conversationHistory.Add(new { role = "model", parts = new[] { new { text = aiText } } });

                // Deep sanitize logic to strip any remaining markdown symbols (*, _, #, headers, list blocks) entirely
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

        // Invokes your custom database routine and intercepts OUT result properties (p_cod and p_mesaj)
        private async Task<(int Cod, string Mesaj)> AddBookAsync(string titlu, string autor, string categorie, string descriere, string isbn, string editura, int anPublicare, string limba, double pretVanzare, double pretChirie, byte[] copertaBytes, int idBibliotecar)
        {
            // Leverages your active application layout configuration management layers (DatabaseConfig)
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

                // Capture output parameter results compiled inside your underlying database procedure schemas
                int codRezultat = pCod.Value != DBNull.Value ? Convert.ToInt32(pCod.Value) : -1;
                string mesajRezultat = pMsg.Value != DBNull.Value ? pMsg.Value.ToString() : "Procedura nu a returnat un mesaj.";

                return (codRezultat, mesajRezultat);
            }
        }
    }

    // Encapsulated UI message helper modeling properties bound directly into XAML items controls
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

                // Uses dynamic dashboard blue style from your library styles resource file
                BackgroundBrush = Application.Current.Resources["PrimaryBlue"] as Brush ?? new SolidColorBrush(Color.FromRgb(37, 99, 235));
                TextBrush = new SolidColorBrush(Colors.White);
                BorderThickness = new Thickness(0);
                BorderBrush = Brushes.Transparent;
            }
            else
            {
                Alignment = HorizontalAlignment.Left;
                BubbleRadius = new CornerRadius(14, 14, 14, 2);

                // Matches default grey/dark cards container styling rules dynamically
                BackgroundBrush = Application.Current.Resources["InputBackground"] as Brush ?? new SolidColorBrush(Color.FromRgb(243, 244, 246));
                TextBrush = Application.Current.Resources["TextPrimary"] as Brush ?? new SolidColorBrush(Color.FromRgb(17, 24, 39));
                BorderThickness = new Thickness(1);
                BorderBrush = Application.Current.Resources["BorderBrushLight"] as Brush ?? new SolidColorBrush(Color.FromRgb(229, 231, 235));
            }
        }
    }
}
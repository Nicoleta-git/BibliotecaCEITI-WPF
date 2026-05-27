using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BibliotecaCEITI
{
    public partial class BookAi : UserControl
    {
        // Secret API Key loaded from system environment variables for production security
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        private const string ModelId = "gemini-3.1-flash-lite";
        private static readonly HttpClient client = new HttpClient();

        // Keeps track of the active session chat context to ensure continuous multi-turn conversations
        private readonly List<object> _conversationHistory = new List<object>();

        // Dynamic visual collection synchronized directly with the UI ItemsControl container
        public ObservableCollection<UiMessage> ChatMessages { get; set; } = new ObservableCollection<UiMessage>();

        // Fallback or active session identity tracking code for database accountability
        private const int CurrentLibrarianId = 1;

        public BookAi()
        {
            InitializeComponent();

            // Optimize network request handshakes to lower latency on API connections
            System.Net.ServicePointManager.Expect100Continue = false;

            // Bind our responsive collection list as the rendering source for the UI chat area
            ChatItemsControl.ItemsSource = ChatMessages;

            // Insert a default helpful greeting context bubble when the system UI starts up
            AddMessageToUi("Salut! Sunt asistentul tău AI. Cu ce te pot ajuta astăzi?", isUser: false);
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            // Simple visual placeholder text clearer on click
            if (input.Text == "Scrie întrebarea ta...")
            {
                input.Text = "";
            }
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            string userPrompt = input.Text.Trim();

            // Guard clause to prevent empty entries or sending standard placeholders
            if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt == "Scrie întrebarea ta...")
            {
                return;
            }

            // Quick technical validation step to check setup health before calling Google servers
            if (string.IsNullOrEmpty(ApiKey))
            {
                AddMessageToUi("Eroare: Cheia API nu a fost găsită. Setați variabila de mediu 'GEMINI_API_KEY'.", isUser: false);
                return;
            }

            try
            {
                // Push user request to screen and clear input instantly to feel fast
                AddMessageToUi(userPrompt, isUser: true);
                input.Text = "";

                // Display a waiting status feedback indicator while the remote worker runs
                var loadingBubble = AddMessageToUi("Se gândește...", isUser: false);
                string rawJson = await GetGeminiRawResponse(userPrompt);
                ChatMessages.Remove(loadingBubble);

                // Run structural execution blocks based on the data received
                await ProcessAiResponse(rawJson);
            }
            catch (Exception ex)
            {
                // Graceful generic exception handler to avoid breaking the application lifecycle
                AddMessageToUi($"System Error: {ex.Message}", isUser: false);
            }
        }

        private UiMessage AddMessageToUi(string text, bool isUser)
        {
            var message = new UiMessage(text, isUser);
            ChatMessages.Add(message);

            // Force visual scrollbar container down to focus on newest data entries asynchronously
            Dispatcher.BeginInvoke(new Action(() => {
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

                // Check standard execution runtime folder structure
                if (File.Exists(path))
                {
                    return File.ReadAllText(path, Encoding.UTF8);
                }

                // Solution space search configuration routing to find fallback instruction text in Dev environments
                string debugFallbackPath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Instructions.txt"));
                if (File.Exists(debugFallbackPath))
                {
                    return File.ReadAllText(debugFallbackPath, Encoding.UTF8);
                }

                // Safe hardcoded behavioral boundaries fallback rule logic string if disk files are missing
                return "Ești asistentul Bibliotecii CEITI. Rolul tău este să ajuți la gestionarea cărților. Poți insera, șterge sau vizualiza categoriile existente apelând uneltele (tools) puse la dispoziție. Dacă utilizatorul cere o imagine, o copertă sau un design, folosește funcția 'TriggerImageGeneration'.";
            }
            catch (Exception)
            {
                // Absolute minimal error proof statement boundary string
                return "Ești asistentul Bibliotecii CEITI. Ajută succint, fără Markdown. Folosește funcția TriggerImageGeneration pentru imagini.";
            }
        }

        private async Task<string> GetGeminiRawResponse(string promptText)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={ApiKey}";

            // Keep tracking historical inputs for structural conversational consistency context
            _conversationHistory.Add(new { role = "user", parts = new[] { new { text = promptText } } });
            string dynamicSystemInstruction = LoadSystemInstructions();

            // Construct the complex tool calling json manifest request tree matching Google's specifications
            var requestBody = new
            {
                contents = _conversationHistory.ToArray(),
                systemInstruction = new { parts = new[] { new { text = dynamicSystemInstruction } } },
                tools = new object[]
                {
                    new {
                        functionDeclarations = new object[]
                        {
                            // Tool mapping to bind automation layer processing directly with stored database structures
                            new {
                                name = "InsertBookIntoDb",
                                description = "Inserează automat o carte nouă în baza de date a bibliotecii prin procedura stocată.",
                                parameters = new {
                                    type = "object",
                                    properties = new {
                                        titlu = new { type = "string", description = "Titlul cărții" },
                                        autor = new { type = "string", description = "Numele complet al autorului" },
                                        categorie = new { type = "string", description = "Denumirea categoriei" },
                                        descriere = new { type = "string", description = "Scurtă descriere a cărții" },
                                        isbn = new { type = "string", description = "Codul ISBN" },
                                        editura = new { type = "string", description = "Numele editurii" },
                                        anPublicare = new { type = "integer", description = "Anul publicării" },
                                        limba = new { type = "string", description = "Limba cărții" },
                                        pretVanzare = new { type = "number", description = "Prețul cărții în MDL" },
                                        pretChirie = new { type = "number", description = "Prețul chiriei în MDL" }
                                    },
                                    required = new[] { "titlu", "autor", "categorie", "anPublicare", "limba", "isbn" }
                                }
                            },
                            // Tool mapping to allow the AI model to request existing database categories
                            new {
                                name = "GetCategoriesFromDb",
                                description = "Obține toate categoriile de cărți disponibile în baza de date pentru a valida inserările.",
                                parameters = new { type = "object", properties = new { } }
                            },
                            // Tool mapping to trigger safe record deletions inside the book database tables
                            new {
                                name = "DeleteBookFromDb",
                                description = "Șterge o carte din baza de date folosind identificatorul unic ID (id_carte).",
                                parameters = new {
                                    type = "object",
                                    properties = new {
                                        idCarte = new { type = "integer", description = "ID-ul unic al cărții pe care vrei să o ștergi" }
                                    },
                                    required = new[] { "idCarte" }
                                }
                            },
                            // Tool mapping to route visual prompt requests to external generative pipelines
                            new {
                                name = "TriggerImageGeneration",
                                description = "Executes when the user wants to design, generate, or view a visual cover or image for any topic.",
                                parameters = new {
                                    type = "object",
                                    properties = new {
                                        artisticPrompt = new { type = "string", description = "A highly descriptive English prompt for the image generator model." },
                                        searchKeyword = new { type = "string", description = "A simple keyword representing the main object (e.g., flowers, robot)." }
                                    },
                                    required = new[] { "artisticPrompt", "searchKeyword" }
                                }
                            }
                        }
                    }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Direct HTTP POST transaction block against the official Google cloud generative endpoint
            var httpResponse = await client.PostAsync(url, content);
            return await httpResponse.Content.ReadAsStringAsync();
        }

        private async Task ProcessAiResponse(string rawJson)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            // Extract the core first token chunk prediction part from the complex returned response tree structure
            var part = root.GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0];

            // Branching verification: Check if the AI decided to execute an intelligent functional routine call instead of speaking
            if (part.TryGetProperty("functionCall", out JsonElement functionCall))
            {
                string functionName = functionCall.GetProperty("name").GetString();
                JsonElement args = functionCall.GetProperty("args");

                // Route Case 1: Automated library record insertion logic pipeline execution
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

                    // Execute transactional SQL interaction via stored routines
                    var dbResult = await AddBookAsync(titlu, autor, categorie, descriere, isbn, editura, anPublicare, limba, pretVanzare, pretChirie, null, CurrentLibrarianId);
                    AddMessageToUi($"[Database Code {dbResult.Cod}]: {dbResult.Mesaj}", isUser: false);
                    _conversationHistory.Add(new { role = "model", parts = new[] { new { text = $"Sistem: Executat inserare carte. Rezultat: {dbResult.Mesaj}" } } });
                }
                // Route Case 2: Fetch and display text categories inside the database instance
                else if (functionName == "GetCategoriesFromDb")
                {
                    List<string> categories = await GetCategoriesAsync();
                    string responseText = "Categoriile disponibile în baza de date sunt:\n" + string.Join("\n", categories);
                    AddMessageToUi(responseText, isUser: false);
                    _conversationHistory.Add(new { role = "model", parts = new[] { new { text = responseText } } });
                }
                // Route Case 3: Process secure single tuple data removal operations
                else if (functionName == "DeleteBookFromDb")
                {
                    int idCarte = args.GetProperty("idCarte").GetInt32();
                    bool success = await DeleteBookAsync(idCarte);

                    string message = success ? $"Cartea cu ID-ul {idCarte} a fost ștearsă cu succes." : $"Nu s-a putut șterge cartea cu ID-ul {idCarte} (posibil să nu existe sau are legături active).";
                    AddMessageToUi(message, isUser: false);
                    _conversationHistory.Add(new { role = "model", parts = new[] { new { text = $"Sistem: {message}" } } });
                }
                // Route Case 4: Visual artwork generation tracking workflow block
                else if (functionName == "TriggerImageGeneration")
                {
                    string artisticPrompt = args.GetProperty("artisticPrompt").GetString();
                    string keyword = args.GetProperty("searchKeyword").GetString();

                    var dynamicBubble = AddMessageToUi($"Se generează imaginea externă bazată pe conceptul '{keyword}'...", isUser: false);

                    // Fetch the raw binary pixel payload matrix stream from our unauthenticated external api pipeline
                    byte[] alternativeImageBytes = await FetchImageFromDedicatedApiAsync(artisticPrompt);

                    // Drop loading banner before trying to show graphics data inside UI bounds
                    ChatMessages.Remove(dynamicBubble);

                    if (alternativeImageBytes != null && alternativeImageBytes.Length > 0)
                    {
                        // Transform byte data streams inside memory directly to fully interactive system graphics resources
                        BitmapImage uiImageSource = ConvertBytesToBitmap(alternativeImageBytes);

                        if (uiImageSource != null)
                        {
                            // Package graphic instance objects inside special UI formatting structures for display rendering
                            var imageMessage = new UiMessage(uiImageSource, isUser: false);
                            ChatMessages.Add(imageMessage);

                            // Recalculate scrolling viewport sizes to include the newly loaded visual canvas structure
                            Dispatcher.BeginInvoke(new Action(() => {
                                ChatScrollViewer.ScrollToBottom();
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }

                        // Feed the successful tool result history turn back into data tracking context array structures
                        _conversationHistory.Add(new { role = "model", parts = new[] { new { text = $"Sistem: Imaginea pentru {keyword} a fost afișată în chat." } } });
                    }
                    else
                    {
                        AddMessageToUi("Eroare la comunicarea cu API-ul alternativ de imagini.", isUser: false);
                    }
                }
            }
            // Branching verification: standard response format containing text streams
            else if (part.TryGetProperty("text", out JsonElement textProp))
            {
                string aiText = textProp.GetString();
                _conversationHistory.Add(new { role = "model", parts = new[] { new { text = aiText } } });

                // Quick clean filter tracking step to erase markdown strings before printing raw characters inside WPF components
                string cleanResult = aiText.Replace("**", "").Replace("__", "").Replace("#", "").Trim();
                AddMessageToUi(cleanResult, isUser: false);
            }
        }

        // Dedicated helper module transforming byte configurations to accessible runtime layout media resources
        private BitmapImage ConvertBytesToBitmap(byte[] rawBytes)
        {
            if (rawBytes == null || rawBytes.Length == 0) return null;

            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(rawBytes))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Immediate synchronous file buffer decoding configuration
                bitmap.EndInit();
            }
            bitmap.Freeze(); // Decouple instance bindings to permit fast rendering across alternate worker runtime loops
            return bitmap;
        }

        // Isolated external image generator integration targeting Pollinations AI framework delivery servers
        private async Task<byte[]> FetchImageFromDedicatedApiAsync(string promptText)
        {
            try
            {
                // Ensure text variables are safe from parsing crashes inside HTTP request strings
                string encodedPrompt = Uri.EscapeDataString(promptText);

                // Set up specialized dimensional parameters for rendering target results (600x800 layout template)
                string standaloneUrl = $"https://image.pollinations.ai/p/{encodedPrompt}?width=600&height=800&seed=42&enhance=true";

                // Standard unauthenticated HTTP GET transaction to capture the direct binary asset content flow
                HttpResponseMessage response = await client.GetAsync(standaloneUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"External API Error: {ex.Message}");
                return null;
            }
        }

        // Traditional structural database routing block connecting directly with target active storage engines
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

                // Process the cover illustration directly using high-capacity SQL database BLOB models
                var pBlob = cmd.Parameters.Add("@p_coperta", MySqlDbType.LongBlob);
                pBlob.Value = (object)copertaBytes ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@p_creat_de", idBibliotecar);

                // Initialize runtime bindings to safely catch directional values returned from procedure routines
                var pIdCarteNou = cmd.Parameters.Add("@p_id_carte_nou", MySqlDbType.UInt32);
                pIdCarteNou.Direction = System.Data.ParameterDirection.Output;
                var pCod = cmd.Parameters.Add("@p_cod", MySqlDbType.Int32);
                pCod.Direction = System.Data.ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@p_mesaj", MySqlDbType.VarChar, 255);
                pMsg.Direction = System.Data.ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                int codRezultat = pCod.Value != DBNull.Value ? Convert.ToInt32(pCod.Value) : -1;
                string mesajRezultat = pMsg.Value != DBNull.Value ? pMsg.Value.ToString() : "Error";

                return (codRezultat, mesajRezultat);
            }
        }

        // Database logic execution step to safely pull a collection of available text categories
        private async Task<List<string>> GetCategoriesAsync()
        {
            var categoriesList = new List<string>();
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (var cmd = new MySqlCommand("SELECT denumire FROM categorii_carti ORDER BY denumire ASC;", conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categoriesList.Add(reader.GetString("denumire"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Reading Error: {ex.Message}");
            }
            return categoriesList;
        }

        // Secure method dealing with direct raw structural row drop queries against the backend engine tables
        private async Task<bool> DeleteBookAsync(int idCarte)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (var cmd = new MySqlCommand("DELETE FROM carti WHERE id = @idCarte;", conn))
                {
                    cmd.Parameters.AddWithValue("@idCarte", idCarte);
                    await conn.OpenAsync();
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Deletion Error: {ex.Message}");
                return false;
            }
        }
    }
}
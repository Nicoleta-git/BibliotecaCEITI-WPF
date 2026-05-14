using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BibliotecaCEITI
{
    public partial class BookAi : UserControl
    {
        // Get the API Key from system environment variables for security
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        // Define the specific AI model ID and create a client for HTTP requests
        private const string ModelId = "gemini-3.1-flash-lite";
        private static readonly HttpClient client = new HttpClient();

        public BookAi()
        {
            InitializeComponent();

            // Disable specific network wait behavior to speed up API calls
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        // Clear the placeholder text when the user clicks on the input box
        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (input.Text == "Scrie întrebarea ta...")
            {
                input.Text = "";
            }
        }

        // Logic for when the Send button is clicked
        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            string userPrompt = input.Text;

            // Validation: Do nothing if the input is empty or just the placeholder
            if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt == "Scrie întrebarea ta...")
            {
                return;
            }

            // Check if the API Key exists before making the request
            if (string.IsNullOrEmpty(ApiKey))
            {
                response.Text = "Eroare: Cheia API nu a fost găsită. Setați variabila de mediu 'GEMINI_API_KEY'.";
                return;
            }

            try
            {
                response.Text = "Se gândește...";
                input.Text = "";

                // Call the API and wait for the result
                string aiResult = await GetGeminiResponse(userPrompt);

                // Remove unwanted Markdown symbols from the AI response
                string cleanResult = aiResult
                    .Replace("**", "")
                    .Replace("__", "")
                    .Replace("#", "")
                    .Replace("* ", "• ")
                    .Trim();

                // Display the final cleaned text
                response.Text = cleanResult;
            }
            catch (Exception ex)
            {
                // Show error message if something goes wrong
                response.Text = $"System Error: {ex.Message}";
            }
        }

        // Method to communicate with Google Gemini API
        private async Task<string> GetGeminiResponse(string promptText)
        {
            // Constructed the API URL with the model ID and API key
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={ApiKey}";

            // Instructions to force the AI to return plain text without formatting
            string systemInstruction = "Raspunde clar si concis in text simplu. NU folosi formatare Markdown (fara bold cu **, fara titluri cu #). ";

            //  JSON object for the request body
            // Creating the "package" (object) exactly as Google's API requires it
            var requestBody = new
            {
                // 1.Google requires a list called "contents" (it can hold multiple messages)
                contents = new[]
                {
                // 2. Each message needs a list of "parts" (can include text, images, or video)
                new {
                    parts = new[]
                    { 
                        // 3. we provide the actual text (system instructions + your question)
                         new { text = systemInstruction + promptText }
                        }
                    }
                  }
                 };

            // JSON example
            //            {
            //                "contents": [
            //                  {
            //                    "parts": [
            //                      {
            //                        "text": "Raspunde clar... Care este capitala Franței?"
            //                      }
            //                   ]
            //                  }
            //                ]
            //              }

            // Convert object to JSON string and prepare the HTTP content
            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send POST request to the API
            var httpResponse = await client.PostAsync(url, content);
            var rawJson = await httpResponse.Content.ReadAsStringAsync();

            // Check if the server returned success
            if (!httpResponse.IsSuccessStatusCode)
            {
                return $"API Error ({httpResponse.StatusCode}): {rawJson}";
            }

            // Parse the complex JSON response to find the actual text message
            using var doc = JsonDocument.Parse(rawJson);
            try
            {
                return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "No response text found."; 
            }
            catch
            {
                return "Eroare la procesarea răspunsului. Date primite: " + rawJson;
            }
        }
    }
}
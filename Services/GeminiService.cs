using System.Net.Http.Json;
using System.Text.Json;

namespace InternshipManagementSystem.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<string> AskAIAsync(string userMessage)
        {
            try
            {
                // ✅ Updated
                //var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-3.8-flash:generateContent?key={_apiKey}";
                var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-3.5-flash-lite:generateContent?key={_apiKey}";

                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = userMessage } } }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(url, payload);

                // Read the raw text so we can see exactly what Google says if it fails
                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // This will show us the exact error from Google!
                    return $"API Error: {rawResponse}";
                }

                using var doc = JsonDocument.Parse(rawResponse);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "No response from AI.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
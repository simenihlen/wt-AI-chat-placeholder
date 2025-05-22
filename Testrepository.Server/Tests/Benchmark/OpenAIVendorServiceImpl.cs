using System.Text;
using System.Text.Json;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Tests.Interface;

namespace Testrepository.Server.Tests.Benchmark;

public class OpenAIVendorServiceImpl : IGptVendorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string VendorName => "OpenAI 4o";

    public OpenAIVendorServiceImpl(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetResponseAsync(List<ChatMessageRequestObject> messages)
    {
        var requestPayload = new
        {
            model = "gpt-4o",
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0.8,
            max_tokens = 300
        };

        var json = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(responseBody);
        return jsonElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}

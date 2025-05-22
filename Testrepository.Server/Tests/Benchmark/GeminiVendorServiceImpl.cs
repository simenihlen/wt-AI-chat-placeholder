using Claudia;
using GenerativeAI;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Tests.Interface;

public class GeminiVendorServiceImpl : IGptVendorService
{
    private readonly IGenerativeModel _geminiModel;

    public string VendorName => "Gemini 1.5 Flash"; // Displayed in your benchmark UI

    public GeminiVendorServiceImpl(IConfiguration config)
    {
        var apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini:ApiKey missing");
        var googleAI = new GoogleAi(apiKey);
        _geminiModel = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");
    }

    public async Task<string> GetResponseAsync(List<ChatMessageRequestObject> messages)
    {
        var prompt = string.Join("\n\n", messages.Select(m => $"{m.Role.ToUpper()}: {m.Content}"));
        var response = await _geminiModel.GenerateContentAsync(prompt);
        return response.Text ?? "[No content returned]";
    }
}
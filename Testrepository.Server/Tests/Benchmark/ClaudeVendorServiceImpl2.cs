using Claudia;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Tests.Interface;

namespace Testrepository.Server.Tests.Benchmark;

public class ClaudeVendorServiceImpl2 : IGptVendorService
{
    private readonly Anthropic _anthropic;

    public string VendorName => "Claude 3 Haiku";

    public ClaudeVendorServiceImpl2(IConfiguration config)
    {
        var apiKey = config["Claude:ApiKey"] ?? throw new ArgumentException("Claude:ApiKey missing");
        _anthropic = new Anthropic
        {
            ApiKey = apiKey
        };
    }

    public async Task<string> GetResponseAsync(List<ChatMessageRequestObject> messages)
    {
        // Convert to Claudia's expected format
        var claudeMessages = messages.Select(m => new Message
        {
            Role = m.Role.ToLower(), // should be "user", "assistant", or "system"
            Content = m.Content
        }).ToList();

        var request = new MessageRequest
        {
            Model = Claudia.Models.Claude3Haiku,
            MaxTokens = 300,
            Temperature = 0.8,
            Messages = claudeMessages.ToArray()
        };

        var response = await _anthropic.Messages.CreateAsync(request);
        return response.Content.FirstOrDefault()?.Text ?? "[No content returned]";

    }
}
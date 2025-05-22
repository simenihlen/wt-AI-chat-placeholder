using Testrepository.Server.Models.Requests;

namespace Testrepository.Server.Tests.Interface;

public interface IGptVendorService
{
    Task<string> GetResponseAsync(List<ChatMessageRequestObject> messages);
    string VendorName { get; }
}

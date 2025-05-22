using System.Diagnostics;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Tests.Interface;

namespace Testrepository.Server.Tests.Benchmark;

public class LatencyBenchmark
{
    private readonly IEnumerable<IGptVendorService> _vendors;

    public LatencyBenchmark(IEnumerable<IGptVendorService> vendors)
    {
        _vendors = vendors;
    }

    public async Task RunBenchmark(string prompt)
    {
        var message = new ChatMessageRequestObject
        {
            Role = "user",
            Content = prompt
        };

        foreach (var vendor in _vendors)
        {
            var messages = new List<ChatMessageRequestObject> { message };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                Console.WriteLine($"Testing {vendor.VendorName}...");
                var response = await vendor.GetResponseAsync(messages);
                stopwatch.Stop();

                Console.WriteLine($"✅ {vendor.VendorName} responded in {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Response: {response.Substring(0, Math.Min(200, response.Length))}...\n");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"❌ {vendor.VendorName} failed: {ex.Message}");
            }
        }
    }
}

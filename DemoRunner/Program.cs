using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Tests.Benchmark;
using Testrepository.Server.Tests.Interface;

// Load configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// All model implementations
var allVendors = new List<IGptVendorService>
{
    new OpenAIVendorServiceImpl(config),      // gpt-4o
    new OpenAIVendorServiceImpl2(config),     // gpt-3.5-turbo
    new OpenAIVendorServiceImpl3(config),     // gpt-4o mini

    new ClaudeVendorServiceImpl(config),      // claude-3.7
    new ClaudeVendorServiceImpl2(config),     // claude-3.5 haiku

    new DeepSeekVendorServiceImpl(config),    // deepseek r1
    new DeepSeekVendorServiceImpl2(config),   // deepseek v3

    new GeminiVendorServiceImpl(config),      // gemini 1.5 flash
    new GeminiVendorServiceImpl2(config),     // gemini 1.5 pro
    new GeminiVendorServiceImpl3(config),     // gemini 2.0 flash
    new GeminiVendorServiceImpl4(config),     // gemini 2.0 flash-lite
};
// Hallucination questions
// key: question, value: expected substring in a correct answer
var factChecks = new Dictionary<string, string>
{
    ["Who is the president of the United States Answer only, no explanation.?"]          = "",
    ["What is the capital of France? Answer only, no explanation.?"]          = "",
    ["Which is the better superhero, Batman or Superman? Answer only, no explanation."]                  = "",
    ["What is (15×3)−(6÷2)+(8^2) − 4 Answer only, no explanation.?"]                            = "",
    ["I travel the world, and I am drunk constantly. Who am I? Answer only, no explanation.?"]                            = "",
    ["How do you make the number one disappear?  Answer only, no explanation."]                       = ""
};

/*
// Prompt config
var smallPrompt = "Compare Javs vs. C#";
var longPrompt = @"
Java and C# are two of the most popular object-oriented programming languages used in enterprise software development today. Both languages offer rich ecosystems, extensive libraries, and powerful development tools. Java, developed by Sun Microsystems in 1995 and now maintained by Oracle, is known for its portability and use of the Java Virtual Machine (JVM), which enables 'write once, run anywhere' compatibility across operating systems. It is widely adopted in Android development, backend services, and large-scale enterprise systems.

C#, developed by Microsoft in 2000 as part of the .NET framework, is designed for Windows development but has become increasingly cross-platform through the .NET Core and .NET 5+ ecosystem. It provides tight integration with Visual Studio, support for modern language features such as LINQ, async/await, and pattern matching, and a vast array of libraries for GUI development, web APIs, and game development (especially via Unity).

When comparing the two, Java emphasizes backward compatibility and a stable runtime, while C# is often quicker to adopt language-level innovations. Both languages compile to intermediate bytecode and run on virtual machines (JVM for Java, CLR for C#), providing strong type safety, automatic garbage collection, and similar object-oriented paradigms. Developers often choose between them based on organizational preference, ecosystem needs, or platform compatibility.

This paragraph compares the two languages in terms of syntax, ecosystem, runtime, and development experience. Both languages offer powerful concurrency support, though Java typically uses threads and executors, while C# uses async/await and the Task Parallel Library (TPL). Tooling is strong for both, with Maven and Gradle dominating Java’s build systems, and MSBuild and NuGet used in the C# world. Both communities are vibrant and active.

".Trim();
var fullPrompt = string.Concat(Enumerable.Repeat(longPrompt + "\n\n", 10)); 

// Prompt to test
var messages = new List<ChatMessageRequestObject>
{
    new() { Role = "user", Content = smallPrompt }
};

// Run benchmark
await RunBenchmark(allVendors, messages);


// ========== Benchmark Method ==========
async Task RunBenchmark(List<IGptVendorService> vendors, List<ChatMessageRequestObject> msgs)
{
    const int warmupRuns = 1;
    const int benchmarkRuns = 5;

    Console.WriteLine("Warming up...");
    foreach (var vendor in vendors)
    {
        try
        {
            await vendor.GetResponseAsync(msgs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Warmup Failed] {vendor.VendorName}: {ex.Message}");
        }
    }

    var timings = new Dictionary<string, double>();

    foreach (var vendor in vendors)
    {
        double total = 0;

        for (int i = 0; i < benchmarkRuns; i++)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                await vendor.GetResponseAsync(msgs);
                sw.Stop();
                total += sw.Elapsed.TotalSeconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] {vendor.VendorName} (Run {i + 1}): {ex.Message}");
                // You can log or skip errors if needed
            }
        }

        double average = total / benchmarkRuns;
        timings[vendor.VendorName] = average;
    }

    Console.WriteLine($"\n=== Benchmark Results over {benchmarkRuns} runs ===");
    foreach (var (vendor, avgTime) in timings)
    {
        Console.WriteLine($"{vendor,-25}: {avgTime:F2}s");
    }
}

*/

Console.WriteLine("\n=== Hallucination Rate Test ===");

foreach (var vendor in allVendors)
{
    int total = factChecks.Count;
    int hallucinations = 0;

    Console.WriteLine($"\nTesting facts on {vendor.VendorName}:");

    foreach (var (question, _) in factChecks)  // We're no longer checking the answer, just printing it
    {
        var msg = new List<ChatMessageRequestObject>
        {
            new() { Role = "user", Content = question }
        };

        string answer;
        try
        {
            answer = await vendor.GetResponseAsync(msg);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [Error] {question}: {ex.Message}");
            hallucinations++;
            continue;
        }

        // Display only the first sentence or short answer
        string shortAnswer = answer.Split('\n')[0];

        Console.WriteLine($"  Q: {question}");
        Console.WriteLine($"     A: {answer}");
    }

  
}

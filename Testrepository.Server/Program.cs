using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Testrepository.Server.Services.ChatService;
using Testrepository.Server.Services.EmbeddingService;
using Testrepository.Server.Services.ProjectService;
using Testrepository.Server.Services.ProjectService.Implementations;
using Testrepository.Server.Services.SessionService;
using Testrepository.Server.Services.SessionService.Implementations;
using Testrepository.Server.Services.StoryService;
using Testrepository.Server.Services.StoryService.Implementations;
using Testrepository.Server.Services.UserService;
using Testrepository.Server.Services.UserService.Implementations;
using Testrepository.Server.Tests.Benchmark;
using Testrepository.Server.Tests.Interface;
using Testrepository.Server.Utils;

var builder = WebApplication.CreateBuilder(args);

Action<DbContextOptionsBuilder> options;

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Dev.json");

    options = ProgramInitUtils.CreateDbActionContextOptionsBuilder(builder.Configuration);
}

else
{
    options = ProgramInitUtils.CreateProductionDbOptions();
}

builder.Services.AddDbContext<DatabaseContext>(options);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<IChatService, OpenAIChatServiceImpl>();
builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingServiceImpl>();
builder.Services.AddScoped<ISessionService, SessionServiceImpl>();
builder.Services.AddScoped<IUserService, UserServiceImpl>();
builder.Services.AddScoped<IStoryService, StoryServiceImpl>();
builder.Services.AddScoped<IProjectService, ProjectServiceImpl>();
builder.Services.AddSingleton<IGptVendorService, OpenAIVendorServiceImpl>();
builder.Services.AddSingleton<IGptVendorService, OpenAIVendorServiceImpl2>();
builder.Services.AddSingleton<IGptVendorService, OpenAIVendorServiceImpl3>();
builder.Services.AddSingleton<IGptVendorService, DeepSeekVendorServiceImpl>();
builder.Services.AddSingleton<IGptVendorService, DeepSeekVendorServiceImpl2>();
builder.Services.AddSingleton<IGptVendorService, ClaudeVendorServiceImpl>();
builder.Services.AddSingleton<IGptVendorService, ClaudeVendorServiceImpl2>();
builder.Services.AddSingleton<IGptVendorService, GeminiVendorServiceImpl>();
builder.Services.AddSingleton<IGptVendorService, GeminiVendorServiceImpl2>();
builder.Services.AddSingleton<IGptVendorService, GeminiVendorServiceImpl3>();
builder.Services.AddSingleton<IGptVendorService, GeminiVendorServiceImpl4>();
builder.Services.AddSingleton<LatencyBenchmark>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = "swagger";
    });
    ProgramInitUtils.EnsureDatabaseCreated(app);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
/*
using (var scope = app.Services.CreateScope())
{
    var benchmark = scope.ServiceProvider.GetRequiredService<LatencyBenchmark>();
    await benchmark.RunBenchmark("What are the main differences between Java and C#?");
}
*/
app.Run();

public partial class Program { }

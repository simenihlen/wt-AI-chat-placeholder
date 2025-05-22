using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Testrepository.Server.Services.SessionService.Implementations;
using Testrepository.Server.Services.UserService.Implementations;
using Testrepository.Server.Utils;

namespace TestProject.Server.Injection
{
    public class CustomWebApplicationFactory<TStartUp> : WebApplicationFactory<Program>
    {
        public string? TestClassName { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (TestClassName == null)
                throw new System.Exception("TestClassName must be set before calling CreateClient");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DatabaseContext));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                ConfigurationManager configurationManager = new ConfigurationManager();
                configurationManager.SetBasePath(AppContext.BaseDirectory);
                configurationManager.AddJsonFile("appsettings.Test.json");
                var options = ProgramInitUtils.CreateDbActionContextOptionsBuilder(configurationManager, TestClassName);
                services.AddDbContext<DatabaseContext>(options);

                // Register services explicitly for testing
                services.AddScoped<OpenAIChatServiceImpl>();
                services.AddScoped<OpenAIEmbeddingServiceImpl>();
                services.AddScoped<SessionServiceImpl>();
                services.AddScoped<UserServiceImpl>();
            });          
        }
        public IServiceScope CreateScope()
        {
            return Services.CreateScope();
        }
    }
}

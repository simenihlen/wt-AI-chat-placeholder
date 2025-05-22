using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Testrepository.Server.Persistence.Internal.GeneratedArtifacts
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            // 🔹 Load the configuration from appsettings.Development.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Dev.json")
                .Build();

            var connectionString = configuration["DatabaseCredentials:ConnectionString"];
            
            Console.WriteLine($"[DEBUG] Loaded Connection String: {connectionString}");

            // 🔹 Configure EF Core to use PostgreSQL with PGVector
            optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

            return new DatabaseContext(optionsBuilder.Options, configuration);
        }
    }
}
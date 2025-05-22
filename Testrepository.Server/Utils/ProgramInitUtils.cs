
using LibGit2Sharp;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;

namespace Testrepository.Server.Utils
{
    public static class ProgramInitUtils
    {
        public static Action<DbContextOptionsBuilder> CreateDbActionContextOptionsBuilder(ConfigurationManager configuration, string testClassName)
        {
            // Build the connection string using NpgsqlConnectionStringBuilder
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(configuration["DatabaseCredentials:ConnectionString"]);


            // Set the database name (you may need to set other properties too)
            builder.Database = $"{GetCurrentBranchName(26)}_{SanitizeString(testClassName, 20)}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            // Pass the complete connection string to UseNpgsql
            return options => options.UseNpgsql(builder.ConnectionString);
        }
        public static Action<DbContextOptionsBuilder> CreateDbActionContextOptionsBuilder(ConfigurationManager configuration)
        {
            // Build the connection string using NpgsqlConnectionStringBuilder
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(configuration["DatabaseCredentials:ConnectionString"]);


            // Set the database name (you may need to set other properties too)
            builder.Database = $"{GetCurrentBranchName(40)}"; 

            // Pass the complete connection string to UseNpgsql
            return options => options.UseNpgsql(builder.ConnectionString);
        }

        private static string SanitizeString(string branchName, int returnedLength)
        {
            // Replace invalid characters with underscores
            string sanitized = Regex.Replace(branchName, @"[^a-zA-Z0-9_]", "_");

            // Ensure the name starts with a letter or underscore
            if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            {
                sanitized = "_" + sanitized;
            }
            sanitized = sanitized.Length > 40 ? sanitized.Substring(0, 40) : sanitized;
            return sanitized;
        }
        public static string GetCurrentBranchName(int returnedBranchNameLength)
        {
            var branchName = FetchBranchName();
            var sanitized = SanitizeString(branchName, returnedBranchNameLength);
            return sanitized;
        }
        private static string FetchBranchName()
        {
            string baseDirectory = AppContext.BaseDirectory;
            DirectoryInfo directoryInfo = new DirectoryInfo(baseDirectory);

            while (directoryInfo != null && !Directory.Exists(Path.Combine(directoryInfo.FullName, ".git")))
            {
                directoryInfo = directoryInfo.Parent!;
            }

            if (directoryInfo == null)
            {
                throw new RepositoryNotFoundException("Git repository not found.");
            }

            using (var repo = new Repository(directoryInfo.FullName))
            {
                return repo.Head.FriendlyName;
            }
        }

        public static void EnsureDatabaseCreated(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.EnsureCreated();
            }
        }

        internal static Action<DbContextOptionsBuilder> CreateProductionDbOptions()
        {
            throw new NotImplementedException();
        }
    }
}

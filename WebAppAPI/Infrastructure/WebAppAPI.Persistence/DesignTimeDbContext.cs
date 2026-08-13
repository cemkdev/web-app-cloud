using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence
{
    public class DesignTimeDbContext : IDesignTimeDbContextFactory<WebAppAPIDbContext>
    {
        public WebAppAPIDbContext CreateDbContext(string[] args)
        {
            string solutionDirectory = FindSolutionDirectory();
            string apiProjectDirectory = Path.Combine(
                solutionDirectory,
                "Presentation",
                "WebAppAPI.API");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            DbContextOptionsBuilder<WebAppAPIDbContext> optionsBuilder = new();

            optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("PostgreSQL"));

            return new WebAppAPIDbContext(optionsBuilder.Options);
        }

        private static string FindSolutionDirectory()
        {
            DirectoryInfo? directory = new(Directory.GetCurrentDirectory());

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "WebAppAPI.sln")))
                    return directory.FullName;

                directory = directory.Parent;
            }

            throw new InvalidOperationException(
                "WebAppAPI solution directory could not be located.");
        }
    }
}
using Microsoft.Extensions.Logging;

namespace DCSHallOfFameApi.Scripts;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- TestScraping    - Test the scraping functionality");
            Console.WriteLine("  dotnet run -- ScrapeAndMigrate - Scrape and migrate data to database");
            Console.WriteLine("  dotnet run -- UpdateNames     - Update all names to proper case");
            Console.WriteLine("  dotnet run -- MigrateToNewProject - Migrate data to new project (dcshalloffame)");
            Console.WriteLine("  dotnet run -- TestFirestore   - Test Firestore connection to target project");
            Console.WriteLine("  dotnet run -- ExtractImages   - Extract image URLs from Google Sites");
            Console.WriteLine("  dotnet run -- MapImages       - Map images from Google Sites to database");
            return;
        }

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "testscraping":
                await TestScraping.RunAsync();
                break;
            case "scrapeandmigrate":
                await ScrapeAndMigrate.RunAsync();
                break;
            case "updatenames":
                await Migration.UpdateNamesToProperCase.RunAsync();
                break;
            case "migratetonewproject":
                await Migration.MigrateToNewProject.RunAsync();
                break;
            case "testfirestore":
                await Migration.TestFirestoreConnection.RunAsync();
                break;
            case "extractimages":
                var imageExtractor = new Migration.ExtractImageUrls();
                await imageExtractor.RunAsync();
                break;
            case "mapimages":
                var imageMapper = new Migration.ManualImageMapping();
                await imageMapper.RunAsync();
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                Console.WriteLine("Available commands: TestScraping, ScrapeAndMigrate, UpdateNames, MigrateToNewProject, TestFirestore, ExtractImages, MapImages");
                break;
        }
    }
}
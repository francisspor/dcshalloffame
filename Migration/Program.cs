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
            default:
                Console.WriteLine($"Unknown command: {command}");
                Console.WriteLine("Available commands: TestScraping, ScrapeAndMigrate");
                break;
        }
    }
}
using Microsoft.Extensions.Logging;

namespace DCSHallOfFameApi.Scripts;

public class TestScraping
{
    public static async Task RunAsync()
    {
        // Create a simple console logger for testing
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<TestScraping>();

        try
        {
            logger.LogInformation("Testing Hall of Fame web scraping...");

            // Test scraping a few members
            var scrapedData = await ScrapeHallOfFameData.ScrapeAllMemberData(logger);

            logger.LogInformation("Scraping completed. Found {Count} members", scrapedData.Count);

            // Display some sample data
            foreach (var kvp in scrapedData.Take(3)) // Show first 3 members
            {
                var name = kvp.Key;
                var (biography, year, achievements) = kvp.Value;

                logger.LogInformation("Member: {Name}", name);
                logger.LogInformation("Induction Year: {Year}", year);
                logger.LogInformation("Biography: {Biography}", biography.Substring(0, Math.Min(100, biography.Length)) + "...");
                logger.LogInformation("Achievements: {Achievements}", string.Join(", ", achievements));
                logger.LogInformation("---");
            }

            // Generate the code for review
            logger.LogInformation("Generating MemberData code:");
            ScrapeHallOfFameData.GenerateMemberDataCode(scrapedData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during test scraping");
        }
    }
}
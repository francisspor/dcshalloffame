using DCSHallOfFameApi.Models;
using DCSHallOfFameApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DCSHallOfFameApi.Scripts;

public class ScrapeAndMigrate
{
    public static async Task RunAsync()
    {
        var host = CreateHostBuilder().Build();
        var firebaseService = host.Services.GetRequiredService<IFirebaseService>();
        var logger = host.Services.GetRequiredService<ILogger<ScrapeAndMigrate>>();

        try
        {
            logger.LogInformation("Starting Hall of Fame scraping and migration...");

            // Step 1: Scrape data from the website
            logger.LogInformation("Scraping data from Hall of Fame website...");
            var scrapedData = await ScrapeHallOfFameData.ScrapeAllMemberData(logger);

            if (scrapedData.Count == 0)
            {
                logger.LogWarning("No data was scraped from the website. Check the website structure or network connection.");
                return;
            }

            logger.LogInformation("Successfully scraped data for {Count} members", scrapedData.Count);

            // Step 2: Generate the MemberData code for manual review
            logger.LogInformation("Generating MemberData code for review:");
            ScrapeHallOfFameData.GenerateMemberDataCode(scrapedData);

            // Step 3: Migrate the scraped data to the database
            logger.LogInformation("Migrating scraped data to database...");
            foreach (var kvp in scrapedData)
            {
                var name = kvp.Key;
                var (biography, inductionYear, achievements) = kvp.Value;

                var member = new HallOfFameMember
                {
                    Name = name,
                    Category = DetermineCategory(name), // You'll need to implement this based on your lists
                    InductionYear = inductionYear,
                    Biography = biography,
                    ImageUrl = "", // Will need to be updated manually
                    Achievements = achievements
                };

                try
                {
                    var id = await firebaseService.CreateMemberAsync(member);
                    logger.LogInformation("Successfully migrated member: {Name} with ID: {Id}", name, id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error migrating member: {Name}", name);
                }
            }

            logger.LogInformation("Scraping and migration completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during scraping and migration");
            throw;
        }
    }

    private static MemberCategory DetermineCategory(string name)
    {
        // Define the staff and alumni lists from your original migration script
        var staffMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ROBERT SHAFER", "BEATRICE ECKLER RASK", "CHARLES GUYDER", "RIT MORENO",
            "JOE BENA", "BRIAN McGARRY", "FRANK DeMASI", "MURIEL CHATTERTON",
            "HOWARD \"CAPPY\" SCHWORM", "JOHN CONWAY"
        };

        return staffMembers.Contains(name) ? MemberCategory.Staff : MemberCategory.Alumni;
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false)
                      .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IFirebaseService, FirebaseService>();
            });
}
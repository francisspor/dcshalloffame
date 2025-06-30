using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using DCSHallOfFameApi.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Migration;

public class UpdateNamesToProperCase
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger _logger;
    private const string CollectionName = "hallOfFameMembers";

    public UpdateNamesToProperCase(string credentialsPath, string projectId, ILogger logger)
    {
        // Set the environment variable for credentials
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        _firestoreDb = FirestoreDb.Create(projectId);
        _logger = logger;
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("Starting name case update migration...");

        // Read configuration from appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var credentialsPath = config["Firebase:CredentialsFile"] ?? "firebase-credentials.json";
        var projectId = config["Firebase:ProjectId"] ?? "dcs-hall-of-fame";

        Console.WriteLine($"Using credentials path: {credentialsPath}");
        Console.WriteLine($"Using project ID: {projectId}");

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<UpdateNamesToProperCase>();

        try
        {
            var migrator = new UpdateNamesToProperCase(credentialsPath, projectId, logger);
            await migrator.UpdateAllNamesAsync();
            Console.WriteLine("Migration completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during migration: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAllNamesAsync()
    {
        _logger.LogInformation("Starting name case update migration...");

        try
        {
            // Get all members
            var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
            var members = snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Member = doc.ConvertTo<HallOfFameMember>()
            }).ToList();

            _logger.LogInformation("Found {Count} members to process", members.Count);

            var updatedCount = 0;
            var unchangedCount = 0;

            foreach (var item in members)
            {
                var originalName = item.Member.Name;
                var properCaseName = ToProperCase(originalName);

                if (originalName != properCaseName)
                {
                    _logger.LogInformation("Updating '{OriginalName}' to '{ProperCaseName}'", originalName, properCaseName);

                    // Update the name
                    item.Member.Name = properCaseName;
                    item.Member.UpdatedAt = DateTime.UtcNow;

                    // Save back to Firestore
                    await _firestoreDb.Collection(CollectionName).Document(item.Id).SetAsync(item.Member);
                    updatedCount++;
                }
                else
                {
                    _logger.LogInformation("Name '{Name}' is already in proper case, skipping", originalName);
                    unchangedCount++;
                }
            }

            _logger.LogInformation("Migration completed successfully!");
            _logger.LogInformation("Updated: {UpdatedCount} members", updatedCount);
            _logger.LogInformation("Unchanged: {UnchangedCount} members", unchangedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during name case update migration");
            throw;
        }
    }

    private static string ToProperCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Use TextInfo to handle proper case conversion
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }
}
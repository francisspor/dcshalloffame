using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using DCSHallOfFameApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Migration;

public class MigrateToNewProject
{
    private readonly FirestoreDb _sourceDb;
    private readonly FirestoreDb _targetDb;
    private readonly ILogger _logger;
    private const string CollectionName = "hallOfFameMembers";

    public MigrateToNewProject(string sourceCredentialsPath, string targetCredentialsPath, ILogger logger)
    {
        // Set up source database (old project) with explicit credentials
        var sourceCredential = GoogleCredential.FromFile(sourceCredentialsPath);
        _sourceDb = new FirestoreDbBuilder
        {
            ProjectId = "dcs-hall-of-fame",
            Credential = sourceCredential
        }.Build();

        // Set up target database (new project) with explicit credentials
        var targetCredential = GoogleCredential.FromFile(targetCredentialsPath);
        _targetDb = new FirestoreDbBuilder
        {
            ProjectId = "dcshalloffame",
            Credential = targetCredential
        }.Build();

        _logger = logger;
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("Starting migration to new project...");

        // Read configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var sourceCredentialsPath = config["Firebase:CredentialsFile"] ?? "firebase-credentials.json";
        var targetCredentialsPath = "firebase-credentials-dcshalloffame.json";

        Console.WriteLine($"Source credentials: {sourceCredentialsPath}");
        Console.WriteLine($"Target credentials: {targetCredentialsPath}");

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<MigrateToNewProject>();

        try
        {
            var migrator = new MigrateToNewProject(sourceCredentialsPath, targetCredentialsPath, logger);
            await migrator.MigrateAllDataAsync();
            Console.WriteLine("Migration completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during migration: {ex.Message}");
            throw;
        }
    }

    public async Task MigrateAllDataAsync()
    {
        _logger.LogInformation("Starting data migration to new project...");

        try
        {
            // Get all members from source database
            _logger.LogInformation("Reading data from source database...");
            var sourceSnapshot = await _sourceDb.Collection(CollectionName).GetSnapshotAsync();
            var sourceMembers = sourceSnapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Member = doc.ConvertTo<HallOfFameMember>()
            }).ToList();

            _logger.LogInformation("Found {Count} members in source database", sourceMembers.Count);

            var migratedCount = 0;
            var errorCount = 0;

            // Migrate each member to target database
            foreach (var item in sourceMembers)
            {
                try
                {
                    _logger.LogInformation("Migrating member: {Name} (ID: {Id})", item.Member.Name, item.Id);

                    // Create new document in target database
                    var docRef = _targetDb.Collection(CollectionName).Document(item.Id);
                    await docRef.SetAsync(item.Member);

                    migratedCount++;
                    _logger.LogInformation("Successfully migrated member: {Name}", item.Member.Name);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex, "Error migrating member {Name} (ID: {Id})", item.Member.Name, item.Id);
                }
            }

            _logger.LogInformation("Migration completed!");
            _logger.LogInformation("Successfully migrated: {MigratedCount} members", migratedCount);
            _logger.LogInformation("Errors: {ErrorCount} members", errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data migration");
            throw;
        }
    }
}
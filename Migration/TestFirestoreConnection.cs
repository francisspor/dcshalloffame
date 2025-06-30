using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using DCSHallOfFameApi.Models;

namespace Migration;

public class TestFirestoreConnection
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Testing Firestore connection to target project...");

        try
        {
            // Set up target database (new project)
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "firebase-credentials-dcshalloffame.json");
            var db = FirestoreDb.Create("dcshalloffame");

            Console.WriteLine("Successfully connected to Firestore database");

            // Try to create a test document in the hallOfFameMembers collection
            var testCollection = db.Collection("hallOfFameMembers");
            var testDoc = testCollection.Document("test-doc");

            // Test with a real HallOfFameMember object
            var testMember = new HallOfFameMember
            {
                Id = "test-doc",
                Name = "Test Member",
                Category = MemberCategory.Alumni,
                GraduationYear = 2020,
                Biography = "This is a test biography",
                ImageUrl = "https://example.com/test.jpg",
                Achievements = new List<string> { "Test Achievement 1", "Test Achievement 2" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Console.WriteLine("Attempting to write test HallOfFameMember to hallOfFameMembers collection...");
            await testDoc.SetAsync(testMember);
            Console.WriteLine("Successfully wrote test HallOfFameMember to hallOfFameMembers collection!");

            // Try to read it back
            Console.WriteLine("Attempting to read test document...");
            var snapshot = await testDoc.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                Console.WriteLine("Successfully read test document!");
                var member = snapshot.ConvertTo<HallOfFameMember>();
                Console.WriteLine($"Member data: Name={member.Name}, Category={member.Category}, GraduationYear={member.GraduationYear}");
            }

            // Clean up - delete the test document
            Console.WriteLine("Cleaning up test document...");
            await testDoc.DeleteAsync();
            Console.WriteLine("Test document deleted successfully!");

            Console.WriteLine("Firestore connection test completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Firestore connection test: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}
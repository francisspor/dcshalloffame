using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace Migration
{
    public class CollectHallOfFameImages
    {
        private readonly FirestoreDb _db;
        private readonly HttpClient _httpClient;

        // Mapping of names from Google Sites to our database names
        private readonly Dictionary<string, string> _nameMapping = new Dictionary<string, string>
        {
            // Staff Hall of Fame
            { "ROBERT SHAFER", "ROBERT SHAFER" },
            { "BEATRICE ECKLER RASK", "BEATRICE ECKLER RASK" },
            { "CHARLES GUYDER", "CHARLES GUYDER" },
            { "RIT MORENO", "RIT MORENO" },
            { "JOE BENA", "JOE BENA" },
            { "BRIAN McGARRY", "BRIAN McGARRY" },
            { "FRANK DeMASI", "FRANK DeMASI" },
            { "MURIEL CHATTERTON", "MURIEL CHATTERTON" },
            { "HOWARD \"CAPPY\" SCHWORM", "HOWARD \"CAPPY\" SCHWORM" },
            { "JOHN CONWAY", "JOHN CONWAY" },

            // Alumni Hall of Fame
            { "RUTH EASTON", "RUTH EASTON" },
            { "MILDRED SCHWORM", "MILDRED SCHWORM" },
            { "JOYCE PUTTERMAN", "JOYCE PUTTERMAN" },
            { "WILLIAM BARNES", "WILLIAM BARNES" },
            { "CHARLES WILBUR", "CHARLES WILBUR" },
            { "WILLIAM DEFOREST", "WILLIAM DEFOREST" },
            { "BRUCE PANAS", "BRUCE PANAS" },
            { "JAMES BREITENSTEIN", "JAMES BREITENSTEIN" },
            { "FREDERICK DYKEMAN", "FREDERICK DYKEMAN" },
            { "LOIS CHATTERTON-ROGERS-WATSON", "LOIS CHATTERTON-ROGERS-WATSON" },
            { "SANDRA SCHLIM", "SANDRA SCHLIM" },
            { "JUDITH ASH BROCKE", "JUDITH ASH BROCKE" },
            { "RAYMOND HAWES", "RAYMOND HAWES" },
            { "RICHARD MURRAY", "RICHARD MURRAY" },
            { "MARK CHATTERTON", "MARK CHATTERTON" },
            { "ALFRED MILLER", "ALFRED MILLER" },
            { "MARTIN WILLIAMS", "MARTIN WILLIAMS" },
            { "STEVEN SCHRADE", "STEVEN SCHRADE" },
            { "DAVID VINCENT", "DAVID VINCENT" },
            { "TIMOTHY GILCHRIEST", "TIMOTHY GILCHRIEST" },
            { "LARRY KEMMER", "LARRY KEMMER" },
            { "JOSEPH MERLI", "JOSEPH MERLI" },
            { "LOIS MILLER", "LOIS MILLER" },
            { "BRUCE EVANS", "BRUCE EVANS" },
            { "BARBARA SALISBURG", "BARBARA SALISBURG" },
            { "DAVID HEISIG", "DAVID HEISIG" },
            { "WENDY GRAVES", "WENDY GRAVES" },
            { "LYLA MEADER", "LYLA MEADER" },
            { "JENNIFER WOLFE", "JENNIFER WOLFE" },
            { "AMY CHRISTMAN", "AMY CHRISTMAN" },
            { "MARY TERRELL", "MARY TERRELL" },
            { "CHRISTOPHER MURRAY", "CHRISTOPHER MURRAY" },
            { "STEPHEN J. DUBNER", "STEPHEN J. DUBNER" },
            { "STEVEN SCRANTON", "STEVEN SCRANTON" },
            { "DONALD LARGETEAU", "DONALD LARGETEAU" },
            { "SHARON LEE CROSIER SMITH", "SHARON LEE CROSIER SMITH" },
            { "JOHN TELOW", "JOHN TELOW" },
            { "BARTON MACDOUGALL", "BARTON MACDOUGALL" },
            { "NICK GWIAZDOWSKI", "NICK GWIAZDOWSKI" },
            { "AMY (WHITBECK) GOLDING", "AMY (WHITBECK) GOLDING" }
        };

        // Known image URLs from Google Sites
        private readonly Dictionary<string, string> _imageUrls = new Dictionary<string, string>
        {
            { "RUTH EASTON", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            // Add more image URLs as we discover them
        };

        public CollectHallOfFameImages()
        {
            // Initialize Firestore
            var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-credentials-dcshalloffame.json");
            var builder = new FirestoreDbBuilder
            {
                ProjectId = "dcshalloffame",
                CredentialsPath = credentialsPath
            };
            _db = builder.Build();

            // Initialize HttpClient
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Starting Hall of Fame image collection...");

            try
            {
                // First, let's try to scrape the Google Sites to find more images
                await ScrapeGoogleSitesImages();

                // Then update our database with the images we found
                await UpdateDatabaseWithImages();

                Console.WriteLine("Image collection completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image collection: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task ScrapeGoogleSitesImages()
        {
            Console.WriteLine("Scraping Google Sites for images...");

            var baseUrl = "https://sites.google.com/duanesburg.org/halloffame/";

            // List of pages to scrape
            var pages = new List<string>
            {
                "staff-hall-of-fame",
                "alumni-hall-of-fame"
            };

            foreach (var page in pages)
            {
                try
                {
                    var url = $"{baseUrl}{page}";
                    Console.WriteLine($"Scraping {url}...");

                    var response = await _httpClient.GetStringAsync(url);

                    // Look for image URLs in the HTML
                    var imageMatches = Regex.Matches(response, @"https://lh[0-9]+\.googleusercontent\.com/[^""\s]+");

                    foreach (Match match in imageMatches)
                    {
                        var imageUrl = match.Value;
                        Console.WriteLine($"Found image URL: {imageUrl}");

                        // Try to determine which person this image belongs to
                        // This is a simplified approach - you might need to refine this
                        var personName = await TryToIdentifyPerson(imageUrl, page);
                        if (!string.IsNullOrEmpty(personName))
                        {
                            _imageUrls[personName] = imageUrl;
                            Console.WriteLine($"Mapped image to: {personName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scraping {page}: {ex.Message}");
                }
            }
        }

        private async Task<string> TryToIdentifyPerson(string imageUrl, string pageType)
        {
            // This is a simplified approach - in reality, you might need to:
            // 1. Download the image and analyze it
            // 2. Look at the surrounding HTML context
            // 3. Use image metadata if available

            // For now, we'll use a basic approach based on the page type
            // You might want to manually map these or use a more sophisticated approach

            return null; // Placeholder - implement based on your needs
        }

        private async Task UpdateDatabaseWithImages()
        {
            Console.WriteLine("Updating database with collected images...");

            var collection = _db.Collection("hallOfFameMembers");
            var snapshot = await collection.GetSnapshotAsync();

            int updatedCount = 0;
            int totalCount = 0;

            foreach (var document in snapshot.Documents)
            {
                totalCount++;
                var data = document.ConvertTo<Dictionary<string, object>>();

                if (data.ContainsKey("name") && data["name"] is string name)
                {
                    if (_imageUrls.ContainsKey(name))
                    {
                        var imageUrl = _imageUrls[name];

                        // Update the document with the image URL
                        var updates = new Dictionary<string, object>
                        {
                            { "imageUrl", imageUrl }
                        };

                        await document.Reference.UpdateAsync(updates);
                        Console.WriteLine($"Updated {name} with image URL");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"No image found for {name}");
                    }
                }
            }

            Console.WriteLine($"Updated {updatedCount} out of {totalCount} members with images");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
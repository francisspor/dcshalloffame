using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

namespace Migration
{
    public class ExtractImageUrls
    {
        private readonly HttpClient _httpClient;

        public ExtractImageUrls()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Extracting image URLs from Google Sites...");

            try
            {
                var baseUrl = "https://sites.google.com/duanesburg.org/halloffame/";

                // List of pages to scrape
                var pages = new List<string>
                {
                    "staff-hall-of-fame",
                    "alumni-hall-of-fame"
                };

                var allImageUrls = new List<string>();

                foreach (var page in pages)
                {
                    try
                    {
                        var url = $"{baseUrl}{page}";
                        Console.WriteLine($"\nScraping {url}...");

                        var response = await _httpClient.GetStringAsync(url);

                        // Look for image URLs in the HTML
                        var imageMatches = Regex.Matches(response, @"https://lh[0-9]+\.googleusercontent\.com/[^""\s]+");

                        Console.WriteLine($"Found {imageMatches.Count} image URLs on {page}:");

                        foreach (Match match in imageMatches)
                        {
                            var imageUrl = match.Value;
                            if (!allImageUrls.Contains(imageUrl))
                            {
                                allImageUrls.Add(imageUrl);
                                Console.WriteLine($"  {imageUrl}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error scraping {page}: {ex.Message}");
                    }
                }

                Console.WriteLine($"\nTotal unique image URLs found: {allImageUrls.Count}");

                // Save to file for manual review
                var outputFile = "extracted_image_urls.txt";
                await File.WriteAllLinesAsync(outputFile, allImageUrls);
                Console.WriteLine($"\nImage URLs saved to: {outputFile}");
                Console.WriteLine("Please manually review and map these URLs to the correct Hall of Fame members.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during extraction: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
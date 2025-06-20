using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace DCSHallOfFameApi.Scripts;

public class ScrapeHallOfFameData
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string BaseUrl = "https://sites.google.com/duanesburg.org/halloffame";
    private static readonly string StaffUrl = $"{BaseUrl}/staff-hall-of-fame";
    private static readonly string AlumniUrl = $"{BaseUrl}/alumni-hall-of-fame";

    // Helper to slugify names for URL construction
    private static string Slugify(string name)
    {
        // Remove special characters, replace spaces with hyphens, and lowercase
        var slug = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9 ]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        return slug;
    }

    public static async Task<Dictionary<string, (string Biography, int InductionYear, List<string> Achievements)>> ScrapeAllMemberData(ILogger logger)
    {
        var memberData = new Dictionary<string, (string Biography, int InductionYear, List<string> Achievements)>();

        logger.LogInformation("Starting web scraping of Hall of Fame data...");

        // Scrape Staff members
        var staffMembers = await ScrapeMembersFromSection(StaffUrl, "Staff Hall of Fame", logger);
        foreach (var member in staffMembers)
        {
            memberData[member.Key] = member.Value;
        }

        // Scrape Alumni members
        var alumniMembers = await ScrapeMembersFromSection(AlumniUrl, "Alumni Hall of Fame", logger);
        foreach (var member in alumniMembers)
        {
            memberData[member.Key] = member.Value;
        }

        logger.LogInformation("Successfully scraped data for {Count} members", memberData.Count);
        return memberData;
    }

    private static async Task<Dictionary<string, (string Biography, int InductionYear, List<string> Achievements)>> ScrapeMembersFromSection(string sectionUrl, string navSection, ILogger logger)
    {
        var members = new Dictionary<string, (string Biography, int InductionYear, List<string> Achievements)>();
        var html = await httpClient.GetStringAsync(sectionUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        logger.LogInformation($"Scraping section: {sectionUrl}");
        logger.LogInformation($"Looking for navigation section: {navSection}");

        // Find all <a> tags in the document
        var allLinks = doc.DocumentNode.SelectNodes("//a");
        if (allLinks == null)
        {
            logger.LogError("No <a> tags found on the page!");
            return members;
        }

        // Filter links that are under the current section and look like member pages
        var memberLinks = new List<(string Name, string Url)>();
        foreach (var link in allLinks)
        {
            var name = link.InnerText.Trim();
            var href = link.GetAttributeValue("href", "");
            // Only consider links that look like member pages (not navigation, not empty, not section root)
            if (!string.IsNullOrEmpty(name)
                && !string.IsNullOrEmpty(href)
                && href.Contains("/halloffame/")
                && !href.EndsWith("staff-hall-of-fame")
                && !href.EndsWith("alumni-hall-of-fame")
                && !name.ToLowerInvariant().Contains("hall of fame")
                && !name.ToLowerInvariant().Contains("home")
                && !name.ToLowerInvariant().Contains("awards")
                && !name.ToLowerInvariant().Contains("timeline")
                && !name.ToLowerInvariant().Contains("nominations")
                && !name.ToLowerInvariant().Contains("more")
                && name.Length > 3)
            {
                // Make URL absolute if it's relative
                var url = href.StartsWith("http") ? href : $"https://sites.google.com{href}";
                memberLinks.Add((name, url));
                logger.LogInformation($"Found member link: {name} -> {url}");
            }
        }

        logger.LogInformation($"Found {memberLinks.Count} member links in {navSection}");

        // For each member, scrape their page
        foreach (var (memberName, memberUrl) in memberLinks)
        {
            logger.LogInformation($"Scraping {memberName} at {memberUrl}");
            var memberInfo = await ScrapeIndividualMember(memberUrl, memberName, logger);
            if (memberInfo != null)
            {
                members[memberName.ToUpper()] = memberInfo.Value;
                logger.LogInformation($"Successfully scraped {memberName}: {memberInfo.Value.Biography.Substring(0, Math.Min(100, memberInfo.Value.Biography.Length))}...");
            }
            else
            {
                logger.LogWarning($"Failed to scrape {memberName}");
            }
        }
        return members;
    }

    private static async Task<(string Biography, int InductionYear, List<string> Achievements)?> ScrapeIndividualMember(string memberUrl, string memberName, ILogger logger)
    {
        try
        {
            logger.LogInformation($"Fetching page for {memberName} at {memberUrl}");
            var html = await httpClient.GetStringAsync(memberUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            logger.LogInformation($"Page loaded for {memberName}, HTML length: {html.Length}");

            // Extract the name (heading)
            var nameNode = doc.DocumentNode.SelectSingleNode("//h1") ?? doc.DocumentNode.SelectSingleNode("//h2");
            var name = nameNode?.InnerText.Trim() ?? memberName;
            logger.LogInformation($"Found name: {name}");

            // Extract the class/descriptor line (immediately after the heading)
            var descriptorNode = nameNode?.SelectSingleNode("following-sibling::*[1]");
            var descriptor = descriptorNode?.InnerText.Trim() ?? string.Empty;
            logger.LogInformation($"Found descriptor: {descriptor}");

            // Try to extract the induction year from the descriptor
            int inductionYear = ExtractInductionYearFromDescriptor(descriptor);
            logger.LogInformation($"Extracted induction year: {inductionYear}");

            // Extract the main biography: all consecutive <p> tags after the descriptor
            var bioBuilder = new List<string>();
            var current = descriptorNode?.SelectSingleNode("following-sibling::*[1]");
            int paragraphCount = 0;
            while (current != null && current.Name == "p")
            {
                var text = current.InnerText.Trim();
                if (!string.IsNullOrEmpty(text) && text.Length > 20)
                {
                    bioBuilder.Add(text);
                    paragraphCount++;
                    logger.LogInformation($"Found paragraph {paragraphCount}: {text.Substring(0, Math.Min(100, text.Length))}...");
                }
                current = current.SelectSingleNode("following-sibling::*[1]");
            }
            var biography = string.Join("\n", bioBuilder);

            if (string.IsNullOrEmpty(biography))
            {
                logger.LogWarning($"No biography found with consecutive <p> tags, trying fallback...");
                // Fallback: get all <p> tags on the page
                var allPs = doc.DocumentNode.SelectNodes("//p");
                if (allPs != null)
                {
                    logger.LogInformation($"Found {allPs.Count} total <p> tags on page");
                    foreach (var p in allPs)
                    {
                        var text = p.InnerText.Trim();
                        if (!string.IsNullOrEmpty(text) && text.Length > 20)
                        {
                            bioBuilder.Add(text);
                            logger.LogInformation($"Fallback paragraph: {text.Substring(0, Math.Min(100, text.Length))}...");
                        }
                    }
                    biography = string.Join("\n", bioBuilder);
                }
            }

            logger.LogInformation($"Final biography length: {biography?.Length ?? 0}");

            // Achievements: use descriptor or a summary from the biography
            var achievements = new List<string>();
            if (!string.IsNullOrEmpty(descriptor))
                achievements.Add(descriptor);
            else if (!string.IsNullOrEmpty(biography))
                achievements.Add("See biography");
            else
                achievements.Add("Inducted into Hall of Fame");

            if (!string.IsNullOrEmpty(biography))
            {
                logger.LogInformation($"Successfully extracted data for {memberName}");
                return (biography, inductionYear, achievements);
            }
            else
            {
                logger.LogWarning($"No biography found for {memberName}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error scraping individual member: {memberName} at {memberUrl}");
        }
        return null;
    }

    private static int ExtractInductionYearFromDescriptor(string descriptor)
    {
        var match = Regex.Match(descriptor, @"(19|20)\\d{2}");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        return DateTime.Now.Year;
    }

    public static void GenerateMemberDataCode(Dictionary<string, (string Biography, int InductionYear, List<string> Achievements)> memberData)
    {
        Console.WriteLine("// Generated MemberData dictionary:");
        Console.WriteLine("private static readonly Dictionary<string, (string Biography, int InductionYear, List<string> Achievements)> MemberData = new()");
        Console.WriteLine("{");

        foreach (var kvp in memberData)
        {
            var name = kvp.Key;
            var (biography, year, achievements) = kvp.Value;
            biography = biography.Replace("\"", "\\\"");
            Console.WriteLine($"    [\"{name}\"] = (\"{biography}\", {year}, new List<string> {{ {string.Join(", ", achievements.Select(a => $"\"{a.Replace("\"", "\\\"")}\""))} }}),");
        }
        Console.WriteLine("};");
    }
}
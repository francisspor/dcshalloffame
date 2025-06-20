# DCS Hall of Fame Scraping and Migration

This directory contains tools for scraping Hall of Fame data from the DCS website and migrating it to the Firebase database.

## Files

- **ScrapeHallOfFameData.cs** - Core web scraping functionality
- **ScrapeAndMigrate.cs** - Main script that scrapes data and migrates it to the database
- **TestScraping.cs** - Test script to verify scraping functionality
- **MigrateHallOfFame.csproj** - Project file with necessary dependencies

## Usage

### Test Scraping (Recommended First Step)
```bash
dotnet run --project Migration/MigrateHallOfFame.csproj -- TestScraping
```

This will test the web scraping functionality and show you what data is being extracted.

### Full Scrape and Migrate
```bash
dotnet run --project Migration/MigrateHallOfFame.csproj -- ScrapeAndMigrate
```

This will:
1. Scrape all Hall of Fame data from the website
2. Generate MemberData code for review
3. Migrate the data to your Firebase database

## Configuration

Make sure you have:
1. Firebase credentials configured in your `appsettings.json`
2. Network access to scrape the Hall of Fame website
3. Proper permissions to write to your Firebase database

## Notes

- The scraper automatically determines if members are Staff or Alumni based on predefined lists
- Image URLs will need to be updated manually after migration
- The script generates MemberData code that you can review before migration
- All scraping is done from the public Hall of Fame website
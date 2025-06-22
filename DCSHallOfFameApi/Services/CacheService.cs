using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using DCSHallOfFameApi.Models;

namespace DCSHallOfFameApi.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _defaultExpiration;
    private readonly TimeSpan _slidingExpiration;
    private readonly bool _enableLogging;

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _configuration = configuration;

        // Get cache settings from configuration
        _defaultExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:DefaultExpirationMinutes", 30));
        _slidingExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:SlidingExpirationMinutes", 10));
        _enableLogging = _configuration.GetValue<bool>("Cache:EnableLogging", true);
    }

    public async Task<List<HallOfFameMember>?> GetCachedMembersAsync(string cacheKey)
    {
        return await Task.FromResult(_memoryCache.Get<List<HallOfFameMember>>(cacheKey));
    }

    public async Task<List<HallOfFameMember>?> GetCachedMembersByCategoryAsync(MemberCategory category)
    {
        var cacheKey = $"members_category_{category}";
        return await Task.FromResult(_memoryCache.Get<List<HallOfFameMember>>(cacheKey));
    }

    public async Task<HallOfFameMember?> GetCachedMemberByIdAsync(string id)
    {
        var cacheKey = $"member_{id}";
        return await Task.FromResult(_memoryCache.Get<HallOfFameMember>(cacheKey));
    }

    public async Task SetCachedMembersAsync(string cacheKey, List<HallOfFameMember> members)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultExpiration,
            SlidingExpiration = _slidingExpiration
        };

        _memoryCache.Set(cacheKey, members, cacheOptions);

        if (_enableLogging)
        {
            _logger.LogInformation("Cached {Count} members with key: {CacheKey} (expires in {ExpirationMinutes} minutes)",
                members.Count, cacheKey, _defaultExpiration.TotalMinutes);
        }

        await Task.CompletedTask;
    }

    public async Task SetCachedMembersByCategoryAsync(MemberCategory category, List<HallOfFameMember> members)
    {
        var cacheKey = $"members_category_{category}";
        await SetCachedMembersAsync(cacheKey, members);
    }

    public async Task SetCachedMemberAsync(string id, HallOfFameMember member)
    {
        var cacheKey = $"member_{id}";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultExpiration,
            SlidingExpiration = _slidingExpiration
        };

        _memoryCache.Set(cacheKey, member, cacheOptions);

        if (_enableLogging)
        {
            _logger.LogInformation("Cached member with ID: {Id} (expires in {ExpirationMinutes} minutes)",
                id, _defaultExpiration.TotalMinutes);
        }

        await Task.CompletedTask;
    }

    public async Task InvalidateAllMemberCachesAsync()
    {
        // Get all cache keys that start with our prefixes
        var cacheKeys = GetCacheKeys();

        foreach (var key in cacheKeys)
        {
            _memoryCache.Remove(key);
        }

        if (_enableLogging)
        {
            _logger.LogInformation("Invalidated {Count} member caches", cacheKeys.Count());
        }

        await Task.CompletedTask;
    }

    public async Task InvalidateCategoryCacheAsync(MemberCategory category)
    {
        var cacheKey = $"members_category_{category}";
        _memoryCache.Remove(cacheKey);

        if (_enableLogging)
        {
            _logger.LogInformation("Invalidated category cache for: {Category}", category);
        }

        await Task.CompletedTask;
    }

    public async Task InvalidateMemberCacheAsync(string id)
    {
        var cacheKey = $"member_{id}";
        _memoryCache.Remove(cacheKey);

        if (_enableLogging)
        {
            _logger.LogInformation("Invalidated member cache for ID: {Id}", id);
        }

        await Task.CompletedTask;
    }

    private IEnumerable<string> GetCacheKeys()
    {
        // Since IMemoryCache doesn't provide a way to enumerate keys,
        // we'll use a simple approach by tracking known patterns
        // In a production environment, you might want to use a more sophisticated approach
        // or switch to a distributed cache like Redis that supports key enumeration

        var keys = new List<string>();

        // Add common cache keys
        keys.Add("all_members");
        keys.Add("members_category_Staff");
        keys.Add("members_category_Alumni");

        return keys;
    }
}
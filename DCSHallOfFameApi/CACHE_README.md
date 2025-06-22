# DCS Hall of Fame API - Caching System

## Overview

The DCS Hall of Fame API now includes a comprehensive caching system to minimize Firestore database hits and improve response times. The caching system uses .NET's built-in `IMemoryCache` with configurable expiration times and automatic cache invalidation.

## Features

### Cache-First Strategy
- All read operations (GET requests) check the cache first
- If data is not in cache, it's fetched from Firestore and cached
- Subsequent requests for the same data are served from cache

### Automatic Cache Invalidation
- When members are created, updated, or deleted, relevant caches are automatically invalidated
- Ensures data consistency between cache and database

### Configurable Settings
- Cache expiration times can be configured in `appsettings.json`
- Logging can be enabled/disabled for cache operations

## Configuration

### appsettings.json
```json
{
  "Cache": {
    "DefaultExpirationMinutes": 30,
    "SlidingExpirationMinutes": 10,
    "EnableLogging": true
  }
}
```

### Settings Explained
- **DefaultExpirationMinutes**: How long cached data remains valid (default: 30 minutes)
- **SlidingExpirationMinutes**: If data is accessed before expiration, extend the cache by this amount (default: 10 minutes)
- **EnableLogging**: Whether to log cache operations (default: true)

## Cache Keys

The system uses the following cache key patterns:
- `all_members` - All Hall of Fame members
- `members_category_{category}` - Members filtered by category (Staff/Alumni)
- `member_{id}` - Individual member by ID

## API Endpoints

### Cache Management (Admin Use)
- `POST /api/v1/HallOfFame/cache/clear` - Clear all caches
- `POST /api/v1/HallOfFame/cache/clear/category/{category}` - Clear cache for specific category
- `POST /api/v1/HallOfFame/cache/clear/member/{id}` - Clear cache for specific member

## Performance Benefits

### Before Caching
- Every API request hits Firestore
- Slower response times
- Higher Firestore costs
- More database load

### After Caching
- First request: Cache miss → Fetch from Firestore → Cache result
- Subsequent requests: Cache hit → Serve from memory (fast)
- Reduced Firestore costs
- Better user experience

## Cache Behavior

### Read Operations
1. Check cache for requested data
2. If found (cache hit): Return cached data immediately
3. If not found (cache miss): Fetch from Firestore, cache result, return data

### Write Operations
1. Perform database operation (create/update/delete)
2. Invalidate relevant caches:
   - All member caches (for create/update/delete)
   - Category-specific caches (for create/update/delete)
   - Individual member cache (for update/delete)

## Monitoring

### Logs
When cache logging is enabled, you'll see logs like:
```
info: Cache hit for all members (30 items)
info: Cache miss for Staff members, fetching from Firestore
info: Cached 15 Staff members (expires in 30 minutes)
info: Invalidated 3 member caches
```

### Cache Statistics
The system logs cache hits and misses, allowing you to monitor cache effectiveness.

## Best Practices

1. **Monitor Cache Hit Rates**: High cache hit rates indicate good performance
2. **Adjust Expiration Times**: Balance between performance and data freshness
3. **Use Cache Management Endpoints**: Clear caches when needed for admin operations
4. **Monitor Memory Usage**: In-memory cache uses application memory

## Future Enhancements

Consider these improvements for production use:
- **Distributed Caching**: Use Redis for multi-instance deployments
- **Cache Warming**: Pre-populate cache on application startup
- **Cache Compression**: Compress cached data to reduce memory usage
- **Cache Statistics**: Add metrics for cache hit/miss ratios
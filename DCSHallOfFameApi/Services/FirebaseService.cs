using Google.Cloud.Firestore;
using DCSHallOfFameApi.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;

namespace DCSHallOfFameApi.Services;

public interface IFirebaseService
{
    Task<List<HallOfFameMember>> GetAllMembersAsync();
    Task<List<HallOfFameMember>> GetMembersByCategoryAsync(MemberCategory category);
    Task<HallOfFameMember?> GetMemberByIdAsync(string id);
    Task<string> CreateMemberAsync(HallOfFameMember member);
    Task UpdateMemberAsync(string id, HallOfFameMember member);
    Task DeleteMemberAsync(string id);
}

public class FirebaseService : IFirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ICacheService _cacheService;
    private readonly ILogger<FirebaseService> _logger;
    private const string CollectionName = "hallOfFameMembers";

    public FirebaseService(IConfiguration configuration, ICacheService cacheService, ILogger<FirebaseService> logger)
    {
        var credentialsPath = configuration["Firebase:CredentialsFile"];
        var credentials = GoogleCredential.FromFile(credentialsPath);
        var projectId = configuration["Firebase:ProjectId"];

        var builder = new FirestoreClientBuilder
        {
            Credential = credentials
        };

        _firestoreDb = FirestoreDb.Create(projectId, builder.Build());
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<HallOfFameMember>> GetAllMembersAsync()
    {
        // Try to get from cache first
        var cachedMembers = await _cacheService.GetCachedMembersAsync("all_members");
        if (cachedMembers != null)
        {
            _logger.LogInformation("Retrieved {Count} members from cache", cachedMembers.Count);
            return cachedMembers;
        }

        // If not in cache, fetch from Firestore
        _logger.LogInformation("Cache miss for all members, fetching from Firestore");
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        var members = snapshot.Documents.Select(doc => doc.ConvertTo<HallOfFameMember>()).ToList();

        // Cache the result
        await _cacheService.SetCachedMembersAsync("all_members", members);

        return members;
    }

    public async Task<List<HallOfFameMember>> GetMembersByCategoryAsync(MemberCategory category)
    {
        // Try to get from cache first
        var cachedMembers = await _cacheService.GetCachedMembersByCategoryAsync(category);
        if (cachedMembers != null)
        {
            _logger.LogInformation("Retrieved {Count} {Category} members from cache", cachedMembers.Count, category);
            return cachedMembers;
        }

        // If not in cache, fetch from Firestore
        _logger.LogInformation("Cache miss for {Category} members, fetching from Firestore", category);
        var snapshot = await _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("category", category)
            .GetSnapshotAsync();
        var members = snapshot.Documents.Select(doc => doc.ConvertTo<HallOfFameMember>()).ToList();

        // Cache the result
        await _cacheService.SetCachedMembersByCategoryAsync(category, members);

        return members;
    }

    public async Task<HallOfFameMember?> GetMemberByIdAsync(string id)
    {
        // Try to get from cache first
        var cachedMember = await _cacheService.GetCachedMemberByIdAsync(id);
        if (cachedMember != null)
        {
            _logger.LogInformation("Retrieved member {Id} from cache", id);
            return cachedMember;
        }

        // If not in cache, fetch from Firestore
        _logger.LogInformation("Cache miss for member {Id}, fetching from Firestore", id);
        var doc = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync();
        var member = doc.Exists ? doc.ConvertTo<HallOfFameMember>() : null;

        // Cache the result if member exists
        if (member != null)
        {
            await _cacheService.SetCachedMemberAsync(id, member);
        }

        return member;
    }

    public async Task<string> CreateMemberAsync(HallOfFameMember member)
    {
        member.CreatedAt = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;

        var docRef = _firestoreDb.Collection(CollectionName).Document();
        member.Id = docRef.Id;

        await docRef.SetAsync(member);

        // Invalidate relevant caches
        await _cacheService.InvalidateAllMemberCachesAsync();
        await _cacheService.InvalidateCategoryCacheAsync(member.Category);

        _logger.LogInformation("Created new member with ID: {Id}, invalidated caches", docRef.Id);
        return docRef.Id;
    }

    public async Task UpdateMemberAsync(string id, HallOfFameMember member)
    {
        member.UpdatedAt = DateTime.UtcNow;
        await _firestoreDb.Collection(CollectionName).Document(id).SetAsync(member);

        // Invalidate relevant caches
        await _cacheService.InvalidateAllMemberCachesAsync();
        await _cacheService.InvalidateCategoryCacheAsync(member.Category);
        await _cacheService.InvalidateMemberCacheAsync(id);

        _logger.LogInformation("Updated member {Id}, invalidated caches", id);
    }

    public async Task DeleteMemberAsync(string id)
    {
        // Get the member first to know its category for cache invalidation
        var member = await GetMemberByIdAsync(id);
        if (member == null)
        {
            throw new InvalidOperationException($"Member with ID {id} not found");
        }

        await _firestoreDb.Collection(CollectionName).Document(id).DeleteAsync();

        // Invalidate relevant caches
        await _cacheService.InvalidateAllMemberCachesAsync();
        await _cacheService.InvalidateCategoryCacheAsync(member.Category);
        await _cacheService.InvalidateMemberCacheAsync(id);

        _logger.LogInformation("Deleted member {Id}, invalidated caches", id);
    }
}
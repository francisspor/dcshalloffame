using DCSHallOfFameApi.Models;

namespace DCSHallOfFameApi.Services;

public interface ICacheService
{
    Task<List<HallOfFameMember>?> GetCachedMembersAsync(string cacheKey);
    Task<List<HallOfFameMember>?> GetCachedMembersByCategoryAsync(MemberCategory category);
    Task<HallOfFameMember?> GetCachedMemberByIdAsync(string id);
    Task SetCachedMembersAsync(string cacheKey, List<HallOfFameMember> members);
    Task SetCachedMembersByCategoryAsync(MemberCategory category, List<HallOfFameMember> members);
    Task SetCachedMemberAsync(string id, HallOfFameMember member);
    Task InvalidateAllMemberCachesAsync();
    Task InvalidateCategoryCacheAsync(MemberCategory category);
    Task InvalidateMemberCacheAsync(string id);
}
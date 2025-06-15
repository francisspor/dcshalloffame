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
    private const string CollectionName = "hallOfFameMembers";

    public FirebaseService(IConfiguration configuration)
    {
        var credentialsPath = configuration["Firebase:CredentialsFile"];
        var credentials = GoogleCredential.FromFile(credentialsPath);
        var projectId = configuration["Firebase:ProjectId"];

        var builder = new FirestoreClientBuilder
        {
            Credential = credentials
        };

        _firestoreDb = FirestoreDb.Create(projectId, builder.Build());
    }

    public async Task<List<HallOfFameMember>> GetAllMembersAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        return snapshot.Documents.Select(doc => doc.ConvertTo<HallOfFameMember>()).ToList();
    }

    public async Task<List<HallOfFameMember>> GetMembersByCategoryAsync(MemberCategory category)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("category", category)
            .GetSnapshotAsync();
        return snapshot.Documents.Select(doc => doc.ConvertTo<HallOfFameMember>()).ToList();
    }

    public async Task<HallOfFameMember?> GetMemberByIdAsync(string id)
    {
        var doc = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync();
        return doc.Exists ? doc.ConvertTo<HallOfFameMember>() : null;
    }

    public async Task<string> CreateMemberAsync(HallOfFameMember member)
    {
        member.CreatedAt = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;

        var docRef = _firestoreDb.Collection(CollectionName).Document();
        member.Id = docRef.Id;

        await docRef.SetAsync(member);
        return docRef.Id;
    }

    public async Task UpdateMemberAsync(string id, HallOfFameMember member)
    {
        member.UpdatedAt = DateTime.UtcNow;
        await _firestoreDb.Collection(CollectionName).Document(id).SetAsync(member);
    }

    public async Task DeleteMemberAsync(string id)
    {
        await _firestoreDb.Collection(CollectionName).Document(id).DeleteAsync();
    }
}
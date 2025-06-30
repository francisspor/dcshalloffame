using System;
using Google.Cloud.Firestore;

namespace DCSHallOfFameApi.Models;

public enum MemberCategory
{
    Staff = 0,
    Alumni = 1
}

[FirestoreData]
public class HallOfFameMember
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("category")]
    public MemberCategory Category { get; set; }  // "Staff" or "Alumni"

    [FirestoreProperty("graduationYear")]
    public int? GraduationYear { get; set; }

    [FirestoreProperty("biography")]
    public string Biography { get; set; } = string.Empty;

    [FirestoreProperty("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [FirestoreProperty("achievements")]
    public List<string> Achievements { get; set; } = new();

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
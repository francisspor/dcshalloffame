using Microsoft.AspNetCore.Authorization;

namespace DCSHallOfFameApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute() : base("AdminOnly")
    {
    }
}
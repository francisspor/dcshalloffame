using Microsoft.AspNetCore.Mvc;
using DCSHallOfFameApi.Services;
using DCSHallOfFameApi.Models;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using DCSHallOfFameApi.Attributes;

namespace DCSHallOfFameApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableCors("AllowFrontend")]
public class TestController : ControllerBase
{
    private readonly IFirebaseService _firebaseService;
    private readonly ILogger<TestController> _logger;

    public TestController(IFirebaseService firebaseService, ILogger<TestController> logger)
    {
        _firebaseService = firebaseService;
        _logger = logger;
    }

    [HttpGet("connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            // Try to get all members (this will test the connection)
            var members = await _firebaseService.GetAllMembersAsync();
            return Ok(new {
                status = "success",
                message = "Successfully connected to Firebase",
                memberCount = members.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Firebase connection");
            return StatusCode(500, new {
                status = "error",
                message = "Failed to connect to Firebase",
                error = ex.Message
            });
        }
    }

    [HttpGet("auth")]
    public IActionResult TestAuth()
    {
        var user = User;
        var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
        var claims = user.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();

        return Ok(new {
            isAuthenticated,
            userName = user.Identity?.Name,
            claims,
            headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }

    [HttpPost("test-member")]
    [AdminOnly]
    public async Task<IActionResult> CreateTestMember()
    {
        try
        {
            var testMember = new HallOfFameMember
            {
                Name = "Test Member",
                Category = MemberCategory.Staff,
                Biography = "This is a test member created to verify Firebase connectivity",
                ImageUrl = "https://example.com/test.jpg",
                Achievements = new List<string> { "Test Achievement" }
            };

            var id = await _firebaseService.CreateMemberAsync(testMember);
            return Ok(new {
                status = "success",
                message = "Successfully created test member",
                id = id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test member");
            return StatusCode(500, new {
                status = "error",
                message = "Failed to create test member",
                error = ex.Message
            });
        }
    }
}
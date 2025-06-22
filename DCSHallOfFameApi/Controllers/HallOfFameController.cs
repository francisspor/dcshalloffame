using Microsoft.AspNetCore.Mvc;
using DCSHallOfFameApi.Models;
using DCSHallOfFameApi.Services;

namespace DCSHallOfFameApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HallOfFameController : ControllerBase
{
    private readonly IFirebaseService _firebaseService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<HallOfFameController> _logger;

    public HallOfFameController(IFirebaseService firebaseService, ICacheService cacheService, ILogger<HallOfFameController> logger)
    {
        _firebaseService = firebaseService;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<HallOfFameMember>>> GetAllMembers()
    {
        try
        {
            var members = await _firebaseService.GetAllMembersAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all members");
            return StatusCode(500, "An error occurred while retrieving members");
        }
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<HallOfFameMember>>> GetMembersByCategory(MemberCategory category)
    {
        try
        {
            var members = await _firebaseService.GetMembersByCategoryAsync(category);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members by category: {Category}", category);
            return StatusCode(500, "An error occurred while retrieving members");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HallOfFameMember>> GetMemberById(string id)
    {
        try
        {
            var member = await _firebaseService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member with ID: {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the member");
        }
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateMember(HallOfFameMember member)
    {
        try
        {
            var id = await _firebaseService.CreateMemberAsync(member);
            return CreatedAtAction(nameof(GetMemberById), new { id }, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new member");
            return StatusCode(500, "An error occurred while creating the member");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(string id, HallOfFameMember member)
    {
        try
        {
            var existingMember = await _firebaseService.GetMemberByIdAsync(id);
            if (existingMember == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateMemberAsync(id, member);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member with ID: {Id}", id);
            return StatusCode(500, "An error occurred while updating the member");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(string id)
    {
        try
        {
            var existingMember = await _firebaseService.GetMemberByIdAsync(id);
            if (existingMember == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteMemberAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting member with ID: {Id}", id);
            return StatusCode(500, "An error occurred while deleting the member");
        }
    }

    // Cache management endpoints (for admin use)
    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearAllCaches()
    {
        try
        {
            await _cacheService.InvalidateAllMemberCachesAsync();
            _logger.LogInformation("All caches cleared manually");
            return Ok(new { message = "All caches cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing caches");
            return StatusCode(500, "An error occurred while clearing caches");
        }
    }

    [HttpPost("cache/clear/category/{category}")]
    public async Task<IActionResult> ClearCategoryCache(MemberCategory category)
    {
        try
        {
            await _cacheService.InvalidateCategoryCacheAsync(category);
            _logger.LogInformation("Category cache cleared manually for: {Category}", category);
            return Ok(new { message = $"Category cache for {category} cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing category cache for: {Category}", category);
            return StatusCode(500, "An error occurred while clearing category cache");
        }
    }

    [HttpPost("cache/clear/member/{id}")]
    public async Task<IActionResult> ClearMemberCache(string id)
    {
        try
        {
            await _cacheService.InvalidateMemberCacheAsync(id);
            _logger.LogInformation("Member cache cleared manually for ID: {Id}", id);
            return Ok(new { message = $"Member cache for {id} cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing member cache for ID: {Id}", id);
            return StatusCode(500, "An error occurred while clearing member cache");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using SearchService.Models;
using SearchService.Services;
using Microsoft.AspNetCore.Authorization;
using subscriptions;
using System.Security.Claims;
using SearchService.DbContexts;
using Microsoft.EntityFrameworkCore;
using SearchService.Entities;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
	private readonly ISearchProvider _searchProvider;
	private readonly ILogger<SearchController> _logger;
	private readonly Subscription.SubscriptionClient _subscriptionClient;
    private readonly SearchDbContext _db;

	public SearchController(ISearchProvider searchProvider, ILogger<SearchController> logger, Subscription.SubscriptionClient subscriptionClient, SearchDbContext db)
	{
		_searchProvider = searchProvider;
		_logger = logger;
		_subscriptionClient = subscriptionClient;
        _db = db;
	}

	[HttpPost]
	[ProducesResponseType(typeof(List<DecisionDto>), 200)]
	public async Task<IActionResult> Search([FromBody] SearchRequest request, CancellationToken cancellationToken)
	{
		if (request.Keywords == null || request.Keywords.Count == 0)
		{
			return BadRequest("Anahtar kelimeler gerekli.");
		}

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		var access = await _subscriptionClient.ValidateFeatureAccessAsync(new ValidateFeatureAccessRequest { UserId = userId, FeatureType = "Search" });
		if (!access.HasAccess) return Forbid(access.Message);

		var results = await _searchProvider.SearchAsync(request.Keywords, cancellationToken);
		_ = _subscriptionClient.ConsumeFeatureAsync(new ConsumeFeatureRequest { UserId = userId, FeatureType = "Search" });

		// Store history
		try
		{
			var history = new SearchHistory
			{
				UserId = userId,
				Keywords = string.Join(",", request.Keywords),
				ResultCount = results.Count,
				CreatedAt = DateTime.UtcNow
			};
			_db.SearchHistories.Add(history);
			await _db.SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Arama geçmişi kaydedilemedi");
		}

		return Ok(results);
	}

	// GET api/search/history?take=20
	[HttpGet("history")]
	[ProducesResponseType(typeof(List<SearchHistoryDto>), 200)]
	public async Task<IActionResult> GetHistory([FromQuery] int take = 20)
	{
		take = Math.Clamp(take, 1, 100);
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		var items = await _db.SearchHistories
			.Where(h => h.UserId == userId)
			.OrderByDescending(h => h.CreatedAt)
			.Take(take)
			.Select(h => new SearchHistoryDto(h.Id, h.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(), h.ResultCount, h.CreatedAt))
			.ToListAsync();
		return Ok(items);
	}

	// POST api/search/save/{decisionId}
	[HttpPost("save/{decisionId:long}")]
	public async Task<IActionResult> SaveDecision(long decisionId)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		var exists = await _db.SavedDecisions.AnyAsync(x => x.UserId == userId && x.DecisionId == decisionId);
		if (exists) return Conflict("Karar zaten kaydedilmiş.");
		_db.SavedDecisions.Add(new SavedDecision { UserId = userId, DecisionId = decisionId, SavedAt = DateTime.UtcNow });
		await _db.SaveChangesAsync();
		return Ok();
	}

	// DELETE api/search/save/{decisionId}
	[HttpDelete("save/{decisionId:long}")]
	public async Task<IActionResult> RemoveSaved(long decisionId)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		var entity = await _db.SavedDecisions.FirstOrDefaultAsync(x => x.UserId == userId && x.DecisionId == decisionId);
		if (entity == null) return NotFound();
		_db.SavedDecisions.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}

	// GET api/search/saved
	[HttpGet("saved")]
	[ProducesResponseType(typeof(List<SavedDecisionDto>), 200)]
	public async Task<IActionResult> GetSaved([FromQuery] int take = 50)
	{
		take = Math.Clamp(take, 1, 200);
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		var items = await _db.SavedDecisions
			.Where(s => s.UserId == userId)
			.OrderByDescending(s => s.SavedAt)
			.Take(take)
			.Select(s => new SavedDecisionDto(s.DecisionId, s.SavedAt))
			.ToListAsync();
		return Ok(items);
	}
}

public record SearchHistoryDto(long Id, List<string> Keywords, int ResultCount, DateTime CreatedAt);
public record SavedDecisionDto(long DecisionId, DateTime SavedAt);



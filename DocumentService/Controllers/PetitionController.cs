using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using subscriptions;
using System.Security.Claims;
using DocumentService.Services;
using DocumentService.Entities;


namespace DocumentService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PetitionController : ControllerBase
{
    private readonly Subscription.SubscriptionClient _subscriptionClient;
    private readonly ILogger<PetitionController> _logger;

    public PetitionController(Subscription.SubscriptionClient subscriptionClient, ILogger<PetitionController> logger)
    {
        _subscriptionClient = subscriptionClient;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] PetitionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var access = await _subscriptionClient.ValidateFeatureAccessAsync(new ValidateFeatureAccessRequest { UserId = userId, FeatureType = "Petition" });
        if (!access.HasAccess) return Forbid(access.Message);

        // TODO: gerçek dilekçe üretim servisini entegre et
        var content = $"Dilekçe taslağı (konu: {request.Topic})";

        await _subscriptionClient.ConsumeFeatureAsync(new ConsumeFeatureRequest { UserId = userId, FeatureType = "Petition" });
        return Ok(new PetitionResponse { Content = content });
    }
}

public record PetitionRequest(string Topic, string CaseText, List<string>? Decisions);
public record PetitionResponse { public string Content { get; set; } = string.Empty; }

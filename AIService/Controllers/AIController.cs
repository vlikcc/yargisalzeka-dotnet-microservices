using AIService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using subscriptions;

namespace AIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly Subscription.SubscriptionClient _subscriptionClient;
        private readonly ILogger<AIController> _logger;

        public AIController(Subscription.SubscriptionClient subscriptionClient, ILogger<AIController> logger)
        {
            _subscriptionClient = subscriptionClient;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Granüler erişim kontrolü CaseAnalysis üzerinden yapılır
            var access = await _subscriptionClient.ValidateFeatureAccessAsync(new ValidateFeatureAccessRequest
            {
                UserId = userId,
                FeatureType = FeatureTypes.CaseAnalysis
            });
            if (!access.HasAccess) return Forbid(access.Message);

            var resultSummary = $"İşlenen metin uzunluğu: {request.Text?.Length ?? 0}";

            // Kullanım kaydı (fire & forget)
            _ = _subscriptionClient.ConsumeFeatureAsync(new ConsumeFeatureRequest
            {
                UserId = userId,
                FeatureType = FeatureTypes.CaseAnalysis
            });

            return Ok(new { Durum = "Analiz Tamamlandı", Ozet = resultSummary });
        }
    }
}

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
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            _logger.LogInformation("Kullanıcının aboneliği kontrol ediliyor: {UserId}", userId);

            var statusResponse = await _subscriptionClient.CheckSubscriptionStatusAsync(
                new CheckStatusRequest { UserId = userId });

            if (!statusResponse.HasActiveSubscription || statusResponse.RemainingCredits <= 0)
            {
                return Forbid("Aktif abonelik veya kalan kredi yok.");
            }

            var resultSummary = $"İşlenen metin uzunluğu: {request.Text?.Length ?? 0}";
            // Burada AI modeli ile analiz işlemi yapılır.

            _logger.LogInformation("Kullanıcı {UserId} için analiz tamamlandı. Kalan kredi: {Credits}", userId, statusResponse.RemainingCredits);

            return Ok(new { Durum = "Analiz Tamamlandı", Ozet = resultSummary, KalanKredi = statusResponse.RemainingCredits - 1 });
        }
    }
}

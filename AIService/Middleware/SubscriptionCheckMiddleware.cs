using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using subscriptions;

namespace AIService.Middleware;

public class SubscriptionCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionCheckMiddleware> _logger;

    public SubscriptionCheckMiddleware(RequestDelegate next, ILogger<SubscriptionCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, Subscription.SubscriptionClient subscriptionClient)
    {
        // Sadece Gemini endpoint'lerini kontrol et (test-token hariç)
        if (context.Request.Path.StartsWithSegments("/api/Gemini") && 
            !context.Request.Path.StartsWithSegments("/api/Gemini/test-token"))
        {
            try
            {
                // JWT token'dan user ID'yi al
                var userId = GetUserIdFromToken(context);
                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "Geçersiz token" });
                    return;
                }

                // Abonelik durumunu kontrol et
                var subscriptionRequest = new CheckStatusRequest { UserId = userId };
                var subscriptionResponse = await subscriptionClient.CheckSubscriptionStatusAsync(subscriptionRequest);

                if (!subscriptionResponse.HasActiveSubscription)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Aktif aboneliğiniz bulunmamaktadır" });
                    return;
                }

                if (subscriptionResponse.RemainingCredits <= 0)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Yeterli krediniz bulunmamaktadır" });
                    return;
                }

                _logger.LogInformation("Kullanıcı {UserId} için abonelik kontrolü başarılı. Kalan kredi: {Credits}", 
                    userId, subscriptionResponse.RemainingCredits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Abonelik kontrolü sırasında hata oluştu");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { error = "Abonelik kontrolü sırasında hata oluştu" });
                return;
            }
        }

        await _next(context);
    }

    private string? GetUserIdFromToken(HttpContext context)
    {
        try
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length);
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            return userIdClaim?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token'dan user ID alınırken hata oluştu");
            return null;
        }
    }
}

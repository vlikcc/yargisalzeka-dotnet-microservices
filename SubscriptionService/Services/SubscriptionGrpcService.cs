using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using subscriptions; // intentionally lowercase per generated proto

namespace SubscriptionService.Services;

public class SubscriptionGrpcService : Subscription.SubscriptionBase
{
    private readonly ILogger<SubscriptionGrpcService> _logger;
    private readonly SubscriptionDbContext _dbContext;

    public SubscriptionGrpcService(ILogger<SubscriptionGrpcService> logger, SubscriptionDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<CheckStatusResponse> CheckSubscriptionStatus(CheckStatusRequest request, ServerCallContext context)
    {
        var sub = await ActiveUserSubscription(request.UserId).FirstOrDefaultAsync();
        if (sub == null)
        {
            return new CheckStatusResponse { HasActiveSubscription = false, RemainingCredits = 0 };
        }
        // For backward compatibility expose case analysis remaining for RemainingCredits
        var remaining = await CalculateRemaining(sub, "CaseAnalysis");
        return new CheckStatusResponse { HasActiveSubscription = sub.IsActive, RemainingCredits = remaining };
    }

    public override async Task<ConsumeFeatureResponse> ConsumeFeature(ConsumeFeatureRequest request, ServerCallContext context)
    {
        var (sub, plan) = await GetActiveSubscriptionWithPlan(request.UserId);
        if (sub == null || plan == null)
        {
            return new ConsumeFeatureResponse { Success = false, Message = "Aktif abonelik bulunamadı" };
        }
        var usage = await GetOrCreateUsage(sub, request.FeatureType);
        var (limit, remaining) = GetLimitAndRemaining(plan, usage, request.FeatureType);
        if (limit >= 0 && remaining <= 0)
        {
            return new ConsumeFeatureResponse { Success = false, Message = "Limit tükendi", RemainingCount = 0 };
        }
        usage.UsedCount += 1;
        usage.LastUsed = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        var newRemaining = limit < 0 ? -1 : limit - usage.UsedCount;
        return new ConsumeFeatureResponse { Success = true, Message = "Kullanım kaydedildi", RemainingCount = newRemaining };
    }

    public override async Task<GetRemainingCreditsResponse> GetRemainingCredits(GetRemainingCreditsRequest request, ServerCallContext context)
    {
        var (sub, plan) = await GetActiveSubscriptionWithPlan(request.UserId);
        if (sub == null || plan == null)
        {
            return new GetRemainingCreditsResponse();
        }
        var keyword = await RemainingFor(plan, sub, "KeywordExtraction");
        var caseAnalysis = await RemainingFor(plan, sub, "CaseAnalysis");
        var search = await RemainingFor(plan, sub, "Search");
        var petition = await RemainingFor(plan, sub, "Petition");
        return new GetRemainingCreditsResponse
        {
            KeywordExtraction = keyword,
            CaseAnalysis = caseAnalysis,
            Search = search,
            Petition = petition
        };
    }

    public override async Task<ValidateFeatureAccessResponse> ValidateFeatureAccess(ValidateFeatureAccessRequest request, ServerCallContext context)
    {
        var (sub, plan) = await GetActiveSubscriptionWithPlan(request.UserId);
        if (sub == null || plan == null)
        {
            return new ValidateFeatureAccessResponse { HasAccess = false, Message = "Aktif abonelik yok" };
        }
        var usage = await GetOrCreateUsage(sub, request.FeatureType);
        var (limit, remaining) = GetLimitAndRemaining(plan, usage, request.FeatureType);
        var hasAccess = limit < 0 || remaining > 0;
        return new ValidateFeatureAccessResponse
        {
            HasAccess = hasAccess,
            Message = hasAccess ? "Erişim onaylandı" : "Limit tükendi",
            RemainingCount = remaining
        };
    }

    public override async Task<AssignTrialResponse> AssignTrialSubscription(AssignTrialRequest request, ServerCallContext context)
    {
        // If user already has an active subscription skip
        var existing = await ActiveUserSubscription(request.UserId).AnyAsync();
        if (existing)
        {
            return new AssignTrialResponse { Success = false, Message = "Kullanıcının aktif aboneliği var" };
        }
        var trialPlan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Trial");
        if (trialPlan == null)
        {
            return new AssignTrialResponse { Success = false, Message = "Trial plan tanımlı değil" };
        }
        var userSub = new UserSubscription
        {
            UserId = request.UserId,
            SubscriptionPlanId = trialPlan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(trialPlan.ValidityDays ?? 3),
            IsActive = true
        };
        _dbContext.UserSubscriptions.Add(userSub);
        await _dbContext.SaveChangesAsync();
        return new AssignTrialResponse { Success = true, Message = "Trial abonelik atandı" };
    }

    private IQueryable<UserSubscription> ActiveUserSubscription(string userId)
    {
        var now = DateTime.UtcNow;
        return _dbContext.UserSubscriptions.Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == userId && s.IsActive && (s.EndDate == null || s.EndDate > now));
    }

    private async Task<(UserSubscription? sub, SubscriptionPlan? plan)> GetActiveSubscriptionWithPlan(string userId)
    {
        var sub = await ActiveUserSubscription(userId).FirstOrDefaultAsync();
        return (sub, sub?.SubscriptionPlan);
    }

    private async Task<int> RemainingFor(SubscriptionPlan plan, UserSubscription sub, string featureType)
    {
        var usage = await GetOrCreateUsage(sub, featureType);
        var (limit, remaining) = GetLimitAndRemaining(plan, usage, featureType);
        return remaining;
    }

    private async Task<UsageTracking> GetOrCreateUsage(UserSubscription sub, string featureType)
    {
        var usage = await _dbContext.UsageTrackings.FirstOrDefaultAsync(u => u.UserSubscriptionId == sub.Id && u.FeatureType == featureType);
        if (usage == null)
        {
            usage = new UsageTracking { UserId = sub.UserId, UserSubscriptionId = sub.Id, FeatureType = featureType, UsedCount = 0, ResetDate = DateTime.UtcNow };
            _dbContext.UsageTrackings.Add(usage);
            await _dbContext.SaveChangesAsync();
        }
        // Monthly reset check (simple)
        if ((DateTime.UtcNow - usage.ResetDate).TotalDays >= 30)
        {
            usage.UsedCount = 0;
            usage.ResetDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        return usage;
    }

    private (int limit, int remaining) GetLimitAndRemaining(SubscriptionPlan plan, UsageTracking usage, string featureType)
    {
        int limit = featureType switch
        {
            "KeywordExtraction" => plan.KeywordExtractionLimit,
            "CaseAnalysis" => plan.CaseAnalysisLimit,
            "Search" => plan.SearchLimit,
            "Petition" => plan.PetitionLimit,
            _ => 0
        };
        if (limit < 0) return (-1, -1);
        var remaining = Math.Max(0, limit - usage.UsedCount);
        return (limit, remaining);
    }

    private async Task<int> CalculateRemaining(UserSubscription sub, string featureType)
    {
        var plan = sub.SubscriptionPlan;
        if (plan == null) return 0;
        var usage = await GetOrCreateUsage(sub, featureType);
        var (limit, remaining) = GetLimitAndRemaining(plan, usage, featureType);
        return limit < 0 ? -1 : remaining;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SubscriptionService;
using SubscriptionService.Services;
using ProtoMsgs = subscriptions;
using Xunit;
using System;
using System.Threading.Tasks;

namespace SubscriptionService.UnitTests;

public class SubscriptionGrpcServiceTests
{
    [Fact]
    public async Task CheckSubscriptionStatus_ReturnsInactive_WhenNotFound()
    {
        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var ctx = new SubscriptionDbContext(options);
        var logger = Mock.Of<ILogger<SubscriptionGrpcService>>();
        var svc = new SubscriptionGrpcService(logger, ctx);

    var resp = await svc.CheckSubscriptionStatus(new ProtoMsgs.CheckStatusRequest{ UserId="user-1" }, null!);
        Assert.False(resp.HasActiveSubscription);
        Assert.Equal(0, resp.RemainingCredits);
    }

    [Fact]
    public async Task CheckSubscriptionStatus_ReturnsActiveAndRemainingDerivedFromUsage()
    {
        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var ctx = new SubscriptionDbContext(options);
        // Seed plan with CaseAnalysisLimit = 10
        var plan = new SubscriptionPlan { Name = "TestPlan", CaseAnalysisLimit = 10, SearchLimit = 10, PetitionLimit = 10, KeywordExtractionLimit = 10 };
        ctx.SubscriptionPlans.Add(plan);
        await ctx.SaveChangesAsync();
        // Create active subscription
        var sub = new UserSubscription { UserId = "user-2", SubscriptionPlanId = plan.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10), IsActive = true };
        ctx.UserSubscriptions.Add(sub);
        await ctx.SaveChangesAsync();
        var logger = Mock.Of<ILogger<SubscriptionGrpcService>>();
        var svc = new SubscriptionGrpcService(logger, ctx);
    var resp = await svc.CheckSubscriptionStatus(new ProtoMsgs.CheckStatusRequest{ UserId="user-2" }, null!);
        Assert.True(resp.HasActiveSubscription);
        Assert.Equal(10, resp.RemainingCredits); // derived from CaseAnalysis limit (no usage yet)
    }
}

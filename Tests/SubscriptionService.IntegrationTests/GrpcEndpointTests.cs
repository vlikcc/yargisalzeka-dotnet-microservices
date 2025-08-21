using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionService;
using ProtoMsgs = subscriptions;
using Xunit;

namespace SubscriptionService.IntegrationTests;

public class GrpcEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public GrpcEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Force Testing environment so Program.cs uses InMemory provider
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ENVIRONMENT", "Testing");
            builder.ConfigureServices(services =>
            {
                // After the host is built we will seed via a scope; nothing else needed here
            });
        });

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
        if (!ctx.SubscriptionPlans.Any(p => p.Name == "ITest"))
        {
            var plan = new SubscriptionPlan { Name = "ITest", CaseAnalysisLimit = 7, SearchLimit = 5, PetitionLimit = 3, KeywordExtractionLimit = 10 };
            ctx.SubscriptionPlans.Add(plan);
            ctx.SaveChanges();
            ctx.UserSubscriptions.Add(new UserSubscription { UserId = "user-x", SubscriptionPlanId = plan.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), IsActive = true });
            ctx.SaveChanges();
        }
    }

    [Fact]
    public async Task CheckSubscriptionStatus_ReturnsSeededCredits()
    {
        var client = _factory.CreateDefaultClient();
        using var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions { HttpClient = client });
    var grpc = new ProtoMsgs.Subscription.SubscriptionClient(channel);
    var resp = await grpc.CheckSubscriptionStatusAsync(new ProtoMsgs.CheckStatusRequest { UserId = "user-x" });
        Assert.True(resp.HasActiveSubscription);
    Assert.Equal(7, resp.RemainingCredits);
    }
}

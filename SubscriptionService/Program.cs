using Microsoft.EntityFrameworkCore;
using subscriptions;
using SubscriptionService;
using SubscriptionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Choose provider based on environment / config//
var useInMemory = builder.Environment.IsEnvironment("Testing") ||
                  builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

if (useInMemory)
{
    builder.Services.AddDbContext<SubscriptionDbContext>(options =>
        options.UseInMemoryDatabase("subscription-test-db"));
}
else
{
    builder.Services.AddDbContext<SubscriptionDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddGrpc();

var app = builder.Build();

// Opsiyonel şema oluşturma
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
    ctx.Database.EnsureCreated();
}

app.MapGrpcService<SubscriptionGrpcService>();
app.MapGet("/", () => "SubscriptionService is running. Use a gRPC client to connect.");

app.Run();

public partial class Program { }

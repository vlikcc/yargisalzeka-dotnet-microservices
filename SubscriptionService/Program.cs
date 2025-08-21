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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migration uygulama (EnsureCreated yerine)
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
    ctx.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<SubscriptionGrpcService>();
app.MapControllers();
app.MapGet("/", () => "SubscriptionService is running. Use a gRPC or HTTP client to connect.");

app.Run();

public partial class Program { }

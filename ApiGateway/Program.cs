using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => "API Gateway running");

// Health check endpoint
app.MapHealthChecks("/health");

await app.UseOcelot();

app.Run();


using Microsoft.EntityFrameworkCore;
using SearchService.DbContexts;
using OpenSearch.Client;
using SearchService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using subscriptions;

namespace SearchService;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(); // Swagger eklendi

		builder.Services.AddDbContext<SearchDbContext>(options =>
			options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
		);

		// JWT auth
		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(o =>
			{
				o.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = false,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "insecure-dev-key"))
				};
			});

		// gRPC client
		builder.Services.AddGrpcClient<Subscription.SubscriptionClient>(o =>
		{
			o.Address = new Uri(builder.Configuration["GrpcServices:SubscriptionUrl"]!);
		});

		// OpenSearch client
		builder.Services.AddSingleton<IOpenSearchClient>(sp =>
		{
			var config = sp.GetRequiredService<IConfiguration>();
			var uri = new Uri(config["OpenSearch:Uri"] ?? "http://localhost:9200");
			var settings = new ConnectionSettings(uri)
				.DisableDirectStreaming();
			var client = new OpenSearchClient(settings);
			return client;
		});

		// Provider selection
		var provider = builder.Configuration["Search:Provider"] ?? "postgres";
		if (provider.Equals("opensearch", StringComparison.OrdinalIgnoreCase))
		{
			builder.Services.AddScoped<ISearchProvider, OpenSearchProvider>();
		}
		else
		{
			builder.Services.AddScoped<ISearchProvider, PostgresSearchProvider>();
		}

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger(); // Swagger middleware eklendi
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "SearchService API V1");
				c.RoutePrefix = string.Empty; // Root'ta açılsın
			});
		}

		// Opsiyonel şema oluşturma (EnsureCreated)
		var ensureCreated = app.Services.GetRequiredService<IConfiguration>().GetValue<bool>("Database:EnsureCreated");
		if (ensureCreated)
		{
			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
				db.Database.EnsureCreated();
			}
		}

		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();

		app.Run();
	}
}

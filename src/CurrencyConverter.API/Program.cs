using CurrencyConverter.Infrastructure.Extensions;
using CurrencyConverter.Application.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Trace;
using Serilog;
using System.Text;
using CurrencyConverter.Infrastructure.Swagger;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;


[ExcludeFromCodeCoverage]
internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Logging
		builder.Host.UseSerilog((context, config) =>
		{
			config.ReadFrom.Configuration(context.Configuration)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.Seq(context.Configuration["Seq:ServerUrl"]!);
		});

		// OpenTelemetry
		builder.Services.AddOpenTelemetry()
			.WithTracing(b => b
				.AddAspNetCoreInstrumentation()
				.AddSource("CurrencyConverter"));

		// API Versioning
		builder.Services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1, 0);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;
		});

		// JWT Authentication
		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = builder.Configuration["Jwt:Issuer"],
					ValidAudience = builder.Configuration["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
				};
			});

		// Services
		builder.Services.AddControllers();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Currency Converter API",
				Version = "v1",
				Description = "A currency conversion API with support for historical rates and real-time conversion",
				Contact = new OpenApiContact
				{
					Name = "Your Name",
					Email = "your.email@example.com"
				}
			});

			// Add JWT Authentication
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
			});

			// Add XML Comments
			var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			options.IncludeXmlComments(xmlPath);

			options.OperationFilter<SwaggerDefaultValues>();

			// Add API versioning display
			options.OperationFilter<SwaggerVersioningFilter>();

			options.ExampleFilters();
		});
		builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

		builder.Services.AddApplicationServices(builder.Configuration);
		builder.Services.AddInfrastructureServices(builder.Configuration);

		var app = builder.Build();

		if(app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseInfrastructureMiddleware();
		app.MapControllers();

		app.Run();
	}
}
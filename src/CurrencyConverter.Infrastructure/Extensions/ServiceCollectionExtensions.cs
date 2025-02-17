using CurrencyConverter.Application.Infrastructure.Abstractions;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Infrastructure.Caching;
using CurrencyConverter.Infrastructure.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Infrastructure.Authentication.Settings;
using CurrencyConverter.Infrastructure.Authentication;
using CurrencyConverter.Infrastructure.HealthChecks;
using CurrencyConverter.Infrastructure.Logging.CustomEnrichers;
using CurrencyConverter.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Serilog.Core;
using CurrencyConverter.Infrastructure.Settings;
using CurrencyConverter.Domain.Common.Settings;
using Microsoft.Extensions.Options;

namespace CurrencyConverter.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddInfrastructureServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddOptions<RateLimitSettings>()
				.Bind(configuration.GetSection("RateLimitSettings"))
				.ValidateDataAnnotations();

			services.AddSingleton<RateLimitSettings>(sp => sp.GetRequiredService<IOptions<RateLimitSettings>>().Value);

			services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
			{
				// Retrieve the Redis configuration from the connection strings.
				var redisConfiguration = configuration.GetConnectionString("Redis");
				return StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfiguration);
			});

			// Redis Cache
			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = configuration.GetConnectionString("Redis");
			});

			// Authentication
			services.AddOptions<JwtSettings>()
				.Bind(configuration.GetSection("Jwt"))
				.ValidateDataAnnotations();

			services.AddSingleton<JwtSettings>(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
			services.AddSingleton<JwtTokenHandler>();

			// HTTP Client with Polly
			services.AddHttpClient<IExchangeRateProvider, FrankfurterApiClient>()
				.AddRetryPolicy()
				.AddCircuitBreakerPolicy();

			services.AddSingleton<ICacheService, RedisCacheService>();

			// Health Checks
			services.AddHealthChecks()
				.AddCheck<ExchangeRateHealthCheck>("exchange_rate_api")
				.AddRedis(configuration.GetConnectionString("Redis")!, "redis");

			// Logging
			services.AddSingleton<ILogEventEnricher, CorrelationIdEnricher>();

			return services;
		}

		public static IApplicationBuilder UseInfrastructureMiddleware(
			this IApplicationBuilder app)
		{
			app.UseMiddleware<ExceptionHandlingMiddleware>();
			app.UseMiddleware<RequestResponseLoggingMiddleware>();
			app.UseMiddleware<ApiThrottlingMiddleware>();

			return app;
		}

		private static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder)
		{
			return builder.AddPolicyHandler(GetRetryPolicy());
		}

		private static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder builder)
		{
			return builder.AddPolicyHandler(GetCircuitBreakerPolicy());
		}

		private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.WaitAndRetryAsync(3, retryAttempt =>
					TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
		}

		private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
		}
	}
}

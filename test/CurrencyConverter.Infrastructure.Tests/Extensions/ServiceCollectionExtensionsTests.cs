using CurrencyConverter.Application.Infrastructure.Abstractions;
using CurrencyConverter.Infrastructure.Authentication.Settings;
using CurrencyConverter.Infrastructure.Authentication;
using CurrencyConverter.Infrastructure.Caching;
using CurrencyConverter.Infrastructure.Logging.CustomEnrichers;
using CurrencyConverter.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace CurrencyConverter.Infrastructure.Tests.Extensions
{
	public class ServiceCollectionExtensionsTests
	{
		[Fact]
		public void AddInfrastructureServices_RegistersExpectedServices()
		{
			// Arrange: Create an in-memory configuration.
			var inMemorySettings = new Dictionary<string, string>
			{
                // Set up RateLimitSettings (adjust key names as required by your settings class)
                {"RateLimitSettings:Limit", "1000"},

                // JwtSettings sample value.
                {"JwtSettings:Secret", "supersecret"}
			};

			// Also set up connection strings.
			var connectionStrings = new Dictionary<string, string>
			{
				{"ConnectionStrings:Redis", "localhost:6379"}
			};

			var configurationBuilder = new ConfigurationBuilder()
				.AddInMemoryCollection(inMemorySettings)
				.AddInMemoryCollection(connectionStrings);
			IConfiguration configuration = configurationBuilder.Build();

			var services = new ServiceCollection();

			services.AddHttpContextAccessor();


			// Act: Call the extension method.
			var returnedServices = services.AddInfrastructureServices(configuration);

			services.RemoveAll(typeof(IConnectionMultiplexer));
			services.AddSingleton<IConnectionMultiplexer>(sp => new Mock<IConnectionMultiplexer>().Object);


			// Assert: Verify the same service collection is returned.
			Assert.Same(services, returnedServices);

			// Build the service provider.
			var provider = services.BuildServiceProvider();

			// --- Options & Settings ---
			// Check that RateLimitSettings options are configured.
			var rateLimitOptions = provider.GetService<IOptions<RateLimitSettings>>();
			Assert.NotNull(rateLimitOptions);
			// Also, RateLimitSettings itself is registered as singleton.
			var rateLimitSettings = provider.GetService<RateLimitSettings>();
			Assert.NotNull(rateLimitSettings);

			// --- Redis Connection Multiplexer ---
			// AddStackExchangeRedisCache registers IDistributedCache.
			var connectionMultiplexer = provider.GetService<IConnectionMultiplexer>();
			Assert.NotNull(connectionMultiplexer);

			// --- Redis Cache ---
			// AddStackExchangeRedisCache registers IDistributedCache.
			var distributedCache = provider.GetService<IDistributedCache>();
			Assert.NotNull(distributedCache);

			// --- Authentication ---
			// JwtSettings are configured and JwtTokenHandler is registered.
			var jwtOptions = provider.GetService<IOptions<JwtSettings>>();
			Assert.NotNull(jwtOptions);
			Assert.NotNull(jwtOptions.Value);
			var jwtHandler = provider.GetService<JwtTokenHandler>();
			Assert.NotNull(jwtHandler);

			// --- HTTP Client ---
			// An HTTP client for IExchangeRateProvider (FrankfurterApiClient) is registered.
			var httpClientFactory = provider.GetService<IHttpClientFactory>();
			Assert.NotNull(httpClientFactory);

			// --- Caching Service ---
			// ICacheService should be registered as RedisCacheService.
			var cacheService = provider.GetService<ICacheService>();
			Assert.NotNull(cacheService);
			Assert.IsType<RedisCacheService>(cacheService);

			// --- Health Checks ---
			// Health checks registration should result in an IHealthCheck.
			var healthCheckService = provider.GetService<HealthCheckService>();
			Assert.NotNull(healthCheckService);

			// --- Logging ---
			// ILogEventEnricher should be registered and be a CorrelationIdEnricher.
			var logEnricher = provider.GetService<Serilog.Core.ILogEventEnricher>();
			Assert.NotNull(logEnricher);
			Assert.IsType<CorrelationIdEnricher>(logEnricher);
		}

		[Fact]
		public void IConnectionMultiplexer_Registration_AttemptsConnectionAndThrowsWhenRedisNotAvailable()
		{
			// Arrange: create a configuration that includes a Redis connection string.
			// Use a connection string that is valid syntactically but points to a non-listening port.
			var inMemorySettings = new Dictionary<string, string>
			{
                // Note: Using a port that is likely not open (e.g. 6380) and abortConnect=false
                {"ConnectionStrings:Redis", "localhost:6380"}
			};

			IConfiguration configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(inMemorySettings)
				.Build();

			var services = new ServiceCollection();

			// Call the extension method to register infrastructure services.
			// This will register IConnectionMultiplexer via the lambda.
			services.AddInfrastructureServices(configuration);

			// Build the service provider.
			var provider = services.BuildServiceProvider();

			// Act & Assert:
			// Since there is likely no Redis server running on port 6380,
			// resolving IConnectionMultiplexer should throw a RedisConnectionException.
			var exception = Assert.Throws<RedisConnectionException>(() =>
			{
				// This line triggers the lambda registration, which calls ConnectionMultiplexer.Connect.
				var multiplexer = provider.GetService<IConnectionMultiplexer>();
			});

			Assert.Contains("It was not possible to connect", exception.Message);
		}

		[Fact]
		public void UseInfrastructureMiddleware_AddsExpectedMiddleware()
		{
			// Arrange: Create a test application builder.
			var services = new ServiceCollection().BuildServiceProvider();
			var builder = new TestApplicationBuilder(services);

			// Act: Call the extension method.
			var returnedBuilder = builder.UseInfrastructureMiddleware();

			// Assert:
			// 1. The same builder instance is returned (fluent API).
			Assert.Same(builder, returnedBuilder);
			// 2. The middleware pipeline should have been built with three middleware registrations.
			//    (ExceptionHandlingMiddleware, RequestResponseLoggingMiddleware, ApiThrottlingMiddleware)
			Assert.Equal(3, builder.MiddlewareRegistrations.Count);
		}

		/// <summary>
		/// A simple test implementation of IApplicationBuilder that records calls to Use().
		/// </summary>
		private class TestApplicationBuilder :IApplicationBuilder
		{
			public IServiceProvider ApplicationServices { get; set; }
			public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();
			public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

			/// <summary>
			/// Records an identifier for each middleware registered.
			/// </summary>
			public List<string> MiddlewareRegistrations { get; } = new List<string>();

			public TestApplicationBuilder(IServiceProvider serviceProvider)
			{
				ApplicationServices = serviceProvider;
			}

			public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
			{
				// Capture the fact that a middleware delegate was added.
				// (Due to the way UseMiddleware<T> works, we may not get the actual type name,
				// so we simply record that a registration occurred.)
				MiddlewareRegistrations.Add("Registered");
				return this;
			}

			public RequestDelegate Build() => context => Task.CompletedTask;

			public IApplicationBuilder New()
			{
				// Create a new TestApplicationBuilder with the same ApplicationServices and copy the Properties.
				var newBuilder = new TestApplicationBuilder(ApplicationServices);
				foreach(var kv in Properties)
				{
					newBuilder.Properties.Add(kv.Key, kv.Value);
				}
				return newBuilder;
			}
		}

	}
}

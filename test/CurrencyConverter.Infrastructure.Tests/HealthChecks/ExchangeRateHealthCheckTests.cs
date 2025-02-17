using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Infrastructure.HealthChecks;
using CurrencyConverter.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.HealthChecks
{
	public class ExchangeRateHealthCheckTests
	{
		private readonly Mock<IExchangeRateProvider> _mockProvider;
		private readonly Mock<ILogger<ExchangeRateHealthCheck>> _mockLogger;
		private readonly ExchangeRateHealthCheck _healthCheck;

		public ExchangeRateHealthCheckTests()
		{
			_mockProvider = new Mock<IExchangeRateProvider>();
			_mockLogger = new Mock<ILogger<ExchangeRateHealthCheck>>();
			_healthCheck = new ExchangeRateHealthCheck(_mockProvider.Object, _mockLogger.Object);
		}

		[Fact]
		public async Task CheckHealthAsync_WhenApiIsHealthy_ReturnsHealthy()
		{
			// Arrange
			_mockProvider.Setup(x => x.GetLatestRatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(ExchangeRateTestHelper.CreateExchangeRate("USD", new Dictionary<string, decimal> { ["EUR"] = 0.85m }));

			// Act
			var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

			// Assert
			result.Status.Should().Be(HealthStatus.Healthy);
		}

		[Fact]
		public async Task CheckHealthAsync_WhenApiThrows_ReturnsUnhealthy()
		{
			// Arrange
			_mockProvider.Setup(x => x.GetLatestRatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("API Error"));

			// Act
			var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

			// Assert
			result.Status.Should().Be(HealthStatus.Unhealthy);
		}
	}
}

using CurrencyConverter.Domain.Services;
using CurrencyConverter.Infrastructure.ExternalServices;
using CurrencyConverter.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.ExternalServices
{
	public class FrankfurterApiClientTests
	{
		private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
		private readonly Mock<ILogger<FrankfurterApiClient>> _mockLogger;
		private readonly Mock<CurrencyValidator> _mockCurrencyValidator;
		private readonly FrankfurterApiClient _client;

		public FrankfurterApiClientTests()
		{
			_mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			_mockLogger = new Mock<ILogger<FrankfurterApiClient>>();
			_mockCurrencyValidator = new Mock<CurrencyValidator>(new Domain.Common.Settings.CurrencySettings());

			var client = new HttpClient(_mockHttpMessageHandler.Object)
			{
				BaseAddress = new Uri("https://api.frankfurter.app/")
			};

			_client = new FrankfurterApiClient(client, _mockLogger.Object, _mockCurrencyValidator.Object);
		}

		[Fact]
		public async Task GetLatestRatesAsync_WhenSuccessful_ReturnsExchangeRate()
		{
			// Arrange
			var response = new FrankfurterResponse
			{
				Base = "EUR",
				Date = DateTime.UtcNow,
				Rates = new Dictionary<string, decimal> { ["USD"] = 1.1m }
			};

			SetupMockHandler(JsonSerializer.Serialize(response));

			// Act
			var result = await _client.GetLatestRatesAsync("EUR");

			// Assert
			result.Should().NotBeNull();
			result.BaseCurrency.Should().Be("EUR");
			result.Rates.Should().ContainKey("USD");
		}

		[Fact]
		public async Task GetLatestRatesAsync_WhenFails_ThrowsException()
		{
			// Arrange
			var response = new FrankfurterResponse();

			SetupMockFailedHandler(JsonSerializer.Serialize(response));

			// Act
			var act = ()=> _client.GetLatestRatesAsync("EUR");

			// Assert
			await act.Should().ThrowAsync<Exception>();
		}

		[Fact]
		public async Task GetHistoricalRatesAsync_WhenSuccessful_ReturnsExchangeRate()
		{
			// Arrange
			var date = DateTime.UtcNow.Date;
			var response = new FrankfurterResponse
			{
				Base = "EUR",
				Date = date,
				Rates = new Dictionary<string, decimal> { ["USD"] = 1.1m }
			};

			SetupMockHandler(JsonSerializer.Serialize(response));

			// Act
			var result = await _client.GetHistoricalRatesAsync("EUR", date);

			// Assert
			result.Should().NotBeNull();
			result.BaseCurrency.Should().Be("EUR");
			result.Date.Should().Be(date);
		}

		[Fact]
		public async Task GetHistoricalRatesAsync_WhenFails_ThrowsException()
		{
			// Arrange
			var date = DateTime.UtcNow.Date;
			var response = new FrankfurterResponse();

			SetupMockFailedHandler(JsonSerializer.Serialize(response));

			// Act
			var act = () => _client.GetHistoricalRatesAsync("EUR", date);

			// Assert
			await act.Should().ThrowAsync<Exception>();
		}

		[Fact]
		public async Task GetHistoricalRatesRangeAsync_WhenSuccessful_ReturnsExchangeRates()
		{
			// Arrange
			var startDate = DateTime.UtcNow.AddDays(-5).Date;
			var endDate = DateTime.UtcNow.Date;
			var response = new FrankfurterTimeSeriesResponse
			{
				Base = "EUR",
				StartDate = startDate,
				EndDate = startDate,
				Rates = new Dictionary<DateTime, Dictionary<string, decimal>>
				{
					[startDate] = new()
					{
						{ "USD" , 1.1m }
					}
				}
			};

			SetupMockHandler(JsonSerializer.Serialize(response));

			// Act
			var result = await _client.GetHistoricalRatesRangeAsync("EUR", startDate, endDate);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result.First().BaseCurrency.Should().Be("EUR");
		}

		[Fact]
		public async Task GetHistoricalRatesRangeAsync_WhenFails_ThrowsException()
		{
			// Arrange
			var startDate = DateTime.UtcNow.AddDays(-5).Date;
			var endDate = DateTime.UtcNow.Date;
			var response = new Dictionary<string, FrankfurterResponse>();

			SetupMockFailedHandler(JsonSerializer.Serialize(response));

			// Act
			var act = () => _client.GetHistoricalRatesRangeAsync("EUR", startDate, endDate);

			// Assert
			await act.Should().ThrowAsync<Exception>();
		}

		private void SetupMockHandler(string content)
		{
			_mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(content)
				});
		}

		private void SetupMockFailedHandler(string content)
		{
			_mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent(content)
				});
		}
	}
}


using CurrencyConverter.Application.Infrastructure.Abstractions;
using CurrencyConverter.TestHelpers;
using CurrencyConverter.Domain.Common.Settings;
using Microsoft.Extensions.Options;

namespace CurrencyConverter.Application.Tests.Services
{
	public class ExchangeRateServiceTests
	{
		private readonly GetExchangeRateRequestValidator _exchangeRateValidator;
		private readonly GetHistoricalRatesRequestValidator _historicalRatesValidator;
		private readonly Mock<ICacheService> _cacheService;
		private readonly Mock<IExchangeRateProvider> _exchangeRateProvider;
		private readonly CurrencyValidator _currencyValidator;
		private readonly Mock<ILogger<ExchangeRateService>> _logger;
		private readonly ExchangeRateService _service;
		private readonly CurrencySettings _settings;

		public ExchangeRateServiceTests()
		{
			_settings = new Domain.Common.Settings.CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};

			_currencyValidator = new CurrencyValidator(_settings);

			_exchangeRateValidator = new GetExchangeRateRequestValidator(_currencyValidator);
			_historicalRatesValidator = new GetHistoricalRatesRequestValidator(_currencyValidator);


			_cacheService = new Mock<ICacheService>();
			_exchangeRateProvider = new Mock<IExchangeRateProvider>();
			_logger = new Mock<ILogger<ExchangeRateService>>();

			_service = new ExchangeRateService(
				_exchangeRateValidator,
				_historicalRatesValidator,
				_cacheService.Object,
				_exchangeRateProvider.Object,
				_currencyValidator,
				_logger.Object);
		}

		[Fact]
		public async Task GetLatestRatesAsync_WhenCacheHasData_ReturnsFromCache()
		{
			// Arrange
			var baseCurrency = "USD";
			var cachedRates = CreateSampleExchangeRate(baseCurrency);

			var expectedExchangeRateResponse = new ExchangeRateResponse
			{
				BaseCurrency = cachedRates.BaseCurrency,
				Date = cachedRates.Date,
				Rates = cachedRates.Rates
			};

			_cacheService.Setup(c => c.GetAsync<ExchangeRate>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(cachedRates);

			// Act
			var result = await _service.GetLatestRatesAsync(baseCurrency);

			// Assert
			result.Should().BeEquivalentTo(expectedExchangeRateResponse);
			_exchangeRateProvider.Verify(p => p.GetLatestRatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public async Task GetLatestRatesAsync_WhenCacheEmpty_FetchesFromProvider()
		{
			// Arrange
			var baseCurrency = "USD";
			var rates = CreateSampleExchangeRate(baseCurrency);

			var expectedExchangeRateResponse = new ExchangeRateResponse
			{
				BaseCurrency = rates.BaseCurrency,
				Date = rates.Date,
				Rates = rates.Rates
			};

			_cacheService.Setup(c => c.GetAsync<ExchangeRate>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((ExchangeRate?)null);

			_exchangeRateProvider.Setup(p => p.GetLatestRatesAsync(baseCurrency, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rates);

			// Act
			var result = await _service.GetLatestRatesAsync(baseCurrency);

			// Assert
			result.Should().BeEquivalentTo(expectedExchangeRateResponse);
			_exchangeRateProvider.Verify(p => p.GetLatestRatesAsync(baseCurrency, It.IsAny<CancellationToken>()), Times.Once);
			_cacheService.Verify(c => c.SetAsync(It.IsAny<string>(), rates, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetLatestRatesAsync_WhenValidationFails_ThrowsValidationException()
		{
			// Arrange
			var failures = new List<ValidationFailure>
			{
				new("BaseCurrency", "Invalid currency")
			};

			// Act
			var act = () => _service.GetLatestRatesAsync("TRY");

			// Assert
			await act.Should().ThrowAsync<Application.Exceptions.ValidationException>()
				.Where(e => e.Errors.ContainsKey("BaseCurrency"));
		}

		[Fact]
		public async Task GetLatestRatesAsync_WhenBaseCurrencyNull_UseDefaultBaseCurrency()
		{
			// Arrange
			string baseCurrency = null;

			var rates = CreateSampleExchangeRate(_settings.DefaultBaseCurrency);

			_cacheService.Setup(c => c.GetAsync<ExchangeRate>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((ExchangeRate?)null);

			_exchangeRateProvider.Setup(p => p.GetLatestRatesAsync(_settings.DefaultBaseCurrency, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rates);

			// Act
			var result = await _service.GetLatestRatesAsync(baseCurrency);

			// Assert
			result.BaseCurrency.Equals(_settings.DefaultBaseCurrency);
		}

		[Fact]
		public async Task GetHistoricalRatesAsync_WhenCacheHasData_ReturnsFromCache()
		{
			// Arrange
			var baseCurrency = "USD";
			var date = DateTime.UtcNow.Date;
			var cachedRates = CreateSampleExchangeRate(baseCurrency);

			var expectedExchangeRateResponse = new ExchangeRateResponse
			{
				BaseCurrency = cachedRates.BaseCurrency,
				Date = cachedRates.Date,
				Rates = cachedRates.Rates
			};

			_cacheService.Setup(c => c.GetAsync<ExchangeRate>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(cachedRates);

			// Act
			var result = await _service.GetHistoricalRatesAsync(baseCurrency, date);

			// Assert
			result.Should().BeEquivalentTo(expectedExchangeRateResponse);
			_exchangeRateProvider.Verify(p => p.GetHistoricalRatesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public async Task GetHistoricalRatesAsync_WhenCacheEmpty_FetchesFromProvider()
		{
			// Arrange
			var baseCurrency = "USD";
			var date = DateTime.UtcNow.Date;
			var rates = CreateSampleExchangeRate(baseCurrency);

			var expectedExchangeRateResponse = new ExchangeRateResponse
			{
				BaseCurrency = rates.BaseCurrency,
				Date = rates.Date,
				Rates = rates.Rates
			};

			_cacheService.Setup(c => c.GetAsync<ExchangeRate>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((ExchangeRate?)null);

			_exchangeRateProvider.Setup(p => p.GetHistoricalRatesAsync(baseCurrency, date, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rates);

			// Act
			var result = await _service.GetHistoricalRatesAsync(baseCurrency, date);

			// Assert
			result.Should().BeEquivalentTo(expectedExchangeRateResponse);
			_exchangeRateProvider.Verify(p => p.GetHistoricalRatesAsync(baseCurrency, date, It.IsAny<CancellationToken>()), Times.Once);
			_cacheService.Verify(c => c.SetAsync(It.IsAny<string>(), rates, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetHistoricalRatesRangeAsync_WhenValidationFails_ThrowsValidationException()
		{
			// Arrange
			var baseCurrency = "TRY";
			var date = DateTime.UtcNow.Date;

			// Act
			var act = () => _service.GetHistoricalRatesAsync(baseCurrency, date);

			// Assert
			await act.Should().ThrowAsync<Application.Exceptions.ValidationException>();
		}

		[Fact]
		public async Task GetHistoricalRatesRangeAsync_WhenStartDateAfterEndDate_ThrowsValidationException()
		{
			// Arrange
			var startDate = DateTime.UtcNow.AddDays(1);
			var endDate = DateTime.UtcNow;

			// Act
			var act = () => _service.GetHistoricalRatesRangeAsync("USD", startDate, endDate, 1, 10);

			// Assert
			await act.Should().ThrowAsync<Application.Exceptions.ValidationException>();
		}

		[Fact]
		public async Task GetHistoricalRatesRangeAsync_WhenRatesNotInCache_ShouldRetrieveFromProviderAndCacheAndLog()
		{
			// Arrange
			var baseCurrency = "EUR";
			var startDate = new DateTime(2023, 1, 1);
			var endDate = new DateTime(2023, 1, 31);
			int page = 1;
			int pageSize = 100;
			var cancellationToken = CancellationToken.None;
			var cacheKey = $"rates:{baseCurrency}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";

			// Fake historical rates returned by the provider.
			var fakeRates = new List<ExchangeRate>
			{
				ExchangeRateTestHelper.CreateExchangeRate("USD", new Dictionary<string, decimal> { ["EUR"] =0.85m }),
				ExchangeRateTestHelper.CreateExchangeRate("USD", new Dictionary<string, decimal> { ["GBP"] = 0.73m })
			};

			var fakeExchangeResponse = fakeRates.Select(r => new ExchangeRateResponse
			{
				BaseCurrency = r.BaseCurrency,
				Date = r.Date,
				Rates = r.Rates
			});


			// Set up the cache service to return null (simulate cache miss).
			_cacheService
				.Setup(cs => cs.GetAsync<IEnumerable<ExchangeRate>>(cacheKey, cancellationToken))
				.ReturnsAsync((IEnumerable<ExchangeRate>)null);

			_exchangeRateProvider
				.Setup(ep => ep.GetHistoricalRatesRangeAsync(baseCurrency, startDate, endDate, cancellationToken))
				.ReturnsAsync(fakeRates);


			// Act
			var result = await _service.GetHistoricalRatesRangeAsync(
				baseCurrency,
				startDate,
				endDate,
				page,
				pageSize,
				cancellationToken);

			// Assert
			Assert.Equal(fakeExchangeResponse.ToList(), result.Items.ToList());

			// Verify that SetAsync was called with the expected cache key and parameters.
			_cacheService.Verify(cs => cs.SetAsync<IEnumerable<ExchangeRate>>(
				cacheKey,
				fakeRates,
				It.Is<TimeSpan>(ts => ts == TimeSpan.FromDays(1)),
				cancellationToken), Times.Once);

			// Verify that the logger logged the expected information message.
			_logger.Verify(
				x => x.Log(
					LogLevel.Information,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((state, t) =>
						state.ToString().Contains("Retrieved and cached historical rates for") &&
						state.ToString().Contains(baseCurrency)),
					null,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Fact]
		public async Task GetHistoricalRatesRangeAsync_WhenCacheHasData_ReturnsFromCache()
		{
			// Arrange
			var baseCurrency = "USD";
			var startDate = DateTime.UtcNow.AddDays(-5);
			var endDate = DateTime.UtcNow;
			var cachedRates = new[] { CreateSampleExchangeRate(baseCurrency) };

			var expectedExchangeRateResponse = cachedRates.Select(r => new ExchangeRateResponse
			{
				BaseCurrency = r.BaseCurrency,
				Date = r.Date,
				Rates = r.Rates
			});

			_cacheService.Setup(c => c.GetAsync<IEnumerable<ExchangeRate>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(cachedRates);

			// Act
			var result = await _service.GetHistoricalRatesRangeAsync(baseCurrency, startDate, endDate, 1, 10);

			// Assert
			result.Items.ToList().Should().BeEquivalentTo(expectedExchangeRateResponse.ToList());
			_exchangeRateProvider.Verify(p => p.GetHistoricalRatesRangeAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		private static ExchangeRate CreateSampleExchangeRate(string baseCurrency)
		{
			return ExchangeRateTestHelper.CreateExchangeRate(baseCurrency);
		}

		
	}
}

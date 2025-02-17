using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.TestHelpers;
using CurrencyConverter.Domain.Common.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Services
{
	public class CurrencyConversionServiceTests
	{
		private readonly ConvertCurrencyRequestValidator _validator;
		private readonly Mock<IExchangeRateService> _exchangeRateService;
		private readonly CurrencyValidator _currencyValidator;
		private readonly Mock<ILogger<CurrencyConversionService>> _logger;
		private readonly CurrencyConversionService _service;

		public CurrencyConversionServiceTests()
		{
			var settings = new Domain.Common.Settings.CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};

			_currencyValidator = new CurrencyValidator(settings);

			_validator =new ConvertCurrencyRequestValidator(_currencyValidator);

			_exchangeRateService = new Mock<IExchangeRateService>();
			_logger = new Mock<ILogger<CurrencyConversionService>>();

			_service = new CurrencyConversionService(
				_validator,
				_exchangeRateService.Object,
				_currencyValidator,
				_logger.Object);
		}

		[Fact]
		public async Task ConvertAsync_WithValidRequest_ReturnsConversion()
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = "USD",
				ToCurrency = "EUR",
				Amount = 100
			};

			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = "USD",
				Date = DateTime.UtcNow,
				Rates = new Dictionary<string, decimal>
				{
					["EUR"] = 0.85m
				}
			};

			_exchangeRateService.Setup(s => s.GetLatestRatesAsync(request.FromCurrency, It.IsAny<CancellationToken>()))
				.ReturnsAsync(exchangeRate);

			// Act
			var result = await _service.ConvertAsync(request);

			// Assert
			result.Should().NotBeNull();
			result.FromCurrency.Should().Be(request.FromCurrency);
			result.ToCurrency.Should().Be(request.ToCurrency);
			result.Amount.Should().Be(request.Amount);
			result.ConvertedAmount.Should().Be(request.Amount * 0.85m);
			result.Rate.Should().Be(0.85m);
		}

		[Fact]
		public async Task ConvertAsync_WhenValidationFails_ThrowsValidationException()
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = "USD",  // Valid currency code
				ToCurrency = "EUR",    // Valid currency code
				Amount = -100          // Invalid amount, will fail validation but not domain rules
			};

			// Act
			var act = () => _service.ConvertAsync(request);

			// Assert
			await act.Should().ThrowAsync<Application.Exceptions.ValidationException>();
		}

		[Fact]
		public async Task ConvertAsync_WhenRateNotFound_ThrowsDomainException()
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = "USD",
				ToCurrency = "EUR",
				Amount = 100
			};

			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = "USD",
				Date = DateTime.UtcNow,
				Rates = new Dictionary<string, decimal>
				{
					["GBP"] = 0.73m,  // Include some other rate, but not EUR
					["JPY"] = 110.0m
				}
			};

			_exchangeRateService.Setup(s => s.GetLatestRatesAsync(request.FromCurrency, It.IsAny<CancellationToken>()))
				.ReturnsAsync(exchangeRate);

			// Act
			var act = () => _service.ConvertAsync(request);

			// Assert
			await act.Should().ThrowAsync<Domain.Exceptions.DomainException>()
				.WithMessage($"No rate found for {request.ToCurrency}");
		}

		[Fact]
		public async Task ConvertAsync_WhenUnexpectedExceptionOccurs_ShouldLogErrorAndRethrow()
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = "EUR",
				ToCurrency = "USD",
				Amount = 100
			};

			// Validator returns valid result
			var validatorMock = new Mock<IValidator<ConvertCurrencyRequest>>();
			validatorMock
				.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			// ExchangeRateService: simulate an exception (for example, a network error)
			var exchangeRateServiceMock = new Mock<IExchangeRateService>();
			var simulatedException = new Exception("Simulated error");
			exchangeRateServiceMock
				.Setup(x => x.GetLatestRatesAsync(request.FromCurrency, It.IsAny<CancellationToken>()))
				.ThrowsAsync(simulatedException);

			// Setup CurrencyValidator using Options.Create to supply settings
			var currencySettings = new CurrencySettings
			{
				DefaultBaseCurrency = "EUR",
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};
			var currencyValidator = new CurrencyValidator(currencySettings);

			// Create a mock logger to capture log calls
			var loggerMock = new Mock<ILogger<CurrencyConversionService>>();

			var service = new CurrencyConversionService(
				validatorMock.Object,
				exchangeRateServiceMock.Object,
				currencyValidator,
				loggerMock.Object);

			// Act & Assert
			var ex = await Assert.ThrowsAsync<Exception>(() => service.ConvertAsync(request));
			Assert.Equal("Simulated error", ex.Message);

			// Verify that logger.LogError was called with the exception and expected message.
			// Since ILogger uses a state object, we can check that its string representation contains our message.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) =>
						v.ToString().Contains("Error converting") &&
						v.ToString().Contains(request.Amount.ToString()) &&
						v.ToString().Contains(request.FromCurrency) &&
						v.ToString().Contains(request.ToCurrency)),
					simulatedException,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}
	}

}

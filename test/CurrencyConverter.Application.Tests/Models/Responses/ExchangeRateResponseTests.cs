using CurrencyConverter.Domain.Common.Settings;
using CurrencyConverter.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Models.Responses
{
	public class ExchangeRateResponseTests
	{
		private readonly CurrencyValidator _validator;

		public ExchangeRateResponseTests()
		{
			var settings = new CurrencySettings
			{
				DefaultBaseCurrency = "EUR",
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};
			_validator = new CurrencyValidator(settings);
		}

		[Fact]
		public void GetRate_WithRestrictedCurrency_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};
			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var act = () => exchangeRate.GetRate("TRY", _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency TRY is restricted");
		}

		[Fact]
		public void GetRate_WithNonexistentCurrency_ShouldReturnNull()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};

			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var rate = exchangeRate.GetRate("GBP", _validator);

			// Assert
			rate.Should().BeNull();
		}

		[Theory]
		[InlineData("USD", true)]
		[InlineData("GBP", false)]
		[InlineData("usd", true)]  // Case insensitive check
		public void HasRate_ShouldReturnCorrectResult(string currency, bool expected)
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};

			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var result = exchangeRate.HasRate(currency);

			// Assert
			result.Should().Be(expected);
		}

	
		[Fact]
		public void GetRate_WithNullCurrencyCode_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};

			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var act = () => exchangeRate.GetRate("", _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code cannot be empty");
		}


		[Theory]
		[InlineData("USD", true)]
		[InlineData("GBP", false)]
		[InlineData("usd", true)]  // Case insensitive check
		public void HasRate_WithValidCurrency_ShouldReturnCorrectResult(string currency, bool expected)
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};
			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var result = exchangeRate.HasRate(currency);

			// Assert
			result.Should().Be(expected);
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData(null)]
		public void HasRate_WithInvalidCurrency_ShouldThrowDomainException(string currency)
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};
			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var act = () => exchangeRate.HasRate(currency);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code cannot be empty");
		}

		[Theory]
		[InlineData("US")]
		[InlineData("USDD")]
		[InlineData("US1")]
		public void HasRate_WithInvalidCurrencyFormat_ShouldThrowDomainException(string currency)
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};
			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var act = () => exchangeRate.HasRate(currency);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage(currency.Length != 3
					? "Currency code must be exactly 3 characters"
					: "Currency code must contain only letters");
		}

		

		[Theory]
		[InlineData("USD", 1.1)]
		[InlineData("usd", 1.1)] // Testing case insensitivity
		public void GetRate_WhenCurrencyExists_ShouldReturnRate(string currency, decimal expectedRate)
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};
			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var rate = exchangeRate.GetRate(currency, _validator);

			// Assert
			rate.Should().Be(expectedRate);
		}

		[Theory]
		[InlineData("GBP")]
		[InlineData("JPY")]
		public void GetRate_WhenCurrencyDoesNotExist_ShouldReturnNull(string currency)
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m
			};
			var exchangeRate = new ExchangeRateResponse()
			{
				BaseCurrency = baseCurrency,
				Date = date,
				Rates = rates
			};

			// Act
			var rate = exchangeRate.GetRate(currency, _validator);

			// Assert
			rate.Should().BeNull();
		}
	}
}

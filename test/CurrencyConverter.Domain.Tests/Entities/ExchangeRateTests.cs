

namespace CurrencyConverter.Domain.Tests.Entities
{
	public class ExchangeRateTests
	{
		private readonly CurrencyValidator _validator;

		public ExchangeRateTests()
		{
			var settings = new CurrencySettings
			{
				DefaultBaseCurrency = "EUR",
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};
			_validator = new CurrencyValidator(settings);
		}

		[Fact]
		public void Create_WithValidData_ShouldCreateExchangeRate()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m,
				["GBP"] = 0.85m
			};

			// Act
			var exchangeRate = ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			exchangeRate.Should().NotBeNull();
			exchangeRate.BaseCurrency.Should().Be(baseCurrency);
			exchangeRate.Date.Should().Be(date);
			exchangeRate.Rates.Should().HaveCount(2);
			exchangeRate.Rates["USD"].Should().Be(1.1m);
			exchangeRate.Rates["GBP"].Should().Be(0.85m);
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		[InlineData(" ")]
		public void Create_WithInvalidBaseCurrency_ShouldThrowDomainException(string baseCurrency)
		{
			// Arrange
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal> { ["USD"] = 1.1m };

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code cannot be empty");
		}

		[Fact]
		public void Create_WithRestrictedBaseCurrency_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "TRY";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal> { ["USD"] = 1.1m };

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency TRY is restricted");
		}

		[Fact]
		public void Create_WithRestrictedCurrencyInRates_ShouldExcludeRestrictedCurrency()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 1.1m,
				["TRY"] = 20.0m
			};

			// Act
			var exchangeRate = ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			exchangeRate.Rates.Should().HaveCount(1);
			exchangeRate.Rates.Should().ContainKey("USD");
			exchangeRate.Rates.Should().NotContainKey("TRY");
		}

		[Theory]
		[InlineData("usd", "USD")]
		[InlineData("GbP", "GBP")]
		[InlineData("eUr", "EUR")]
		public void Create_WithDifferentCaseCurrencies_ShouldNormalizeToCaps(string input, string expected)
		{
			// Arrange
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal> { [input] = 1.1m };

			// Act
			var exchangeRate = ExchangeRate.Create("EUR", date, rates, _validator);

			// Assert
			exchangeRate.Rates.Should().ContainKey(expected);
		}

		[Fact]
		public void Create_WithZeroRate_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = 0m
			};

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Rate for USD must be greater than zero");
		}

		[Fact]
		public void Create_WithNegativeRate_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["USD"] = -1.1m
			};

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Rate for USD must be greater than zero");
		}

		[Fact]
		public void Create_WithNullRatesDictionary_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			Dictionary<string, decimal> rates = null;

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Rates cannot be empty");
		}

		[Fact]
		public void Create_WithEmptyRatesDictionary_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>();

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Rates cannot be empty");
		}

		
		[Fact]
		public void Create_WithInvalidCurrencyInRates_ShouldThrowDomainException()
		{
			// Arrange
			var baseCurrency = "EUR";
			var date = DateTime.UtcNow;
			var rates = new Dictionary<string, decimal>
			{
				["INVALID"] = 1.1m
			};

			// Act
			var act = () => ExchangeRate.Create(baseCurrency, date, rates, _validator);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code must be exactly 3 characters");
		}
	}
}

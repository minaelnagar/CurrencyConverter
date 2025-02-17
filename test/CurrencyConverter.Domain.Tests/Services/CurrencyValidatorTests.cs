namespace CurrencyConverter.Domain.Tests.Services
{
	public class CurrencyValidatorTests
	{
		[Fact]
		public void IsRestricted_WithRestrictedCurrency_ShouldReturnTrue()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var result = validator.IsRestricted("TRY");

			// Assert
			result.Should().BeTrue();
		}

		[Fact]
		public void IsRestricted_WithNonRestrictedCurrency_ShouldReturnFalse()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var result = validator.IsRestricted("USD");

			// Assert
			result.Should().BeFalse();
		}

		[Theory]
		[InlineData("EUR1")]
		[InlineData("E")]
		[InlineData("EURO")]
		public void IsRestricted_WithInvalidCurrency_ShouldThrowDomainException(string currency)
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.IsRestricted(currency);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage(currency.Length != 3
					? "Currency code must be exactly 3 characters"
					: "Currency code must contain only letters");
		}

		[Fact]
		public void GetDefaultBaseCurrency_ShouldReturnConfiguredValue()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				DefaultBaseCurrency = "USD"
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var result = validator.GetDefaultBaseCurrency();

			// Assert
			result.Should().Be("USD");
		}

		[Fact]
		public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
		{
			// Act
			var act = () => new CurrencyValidator(null);

			// Assert
			act.Should().Throw<ArgumentNullException>()
				.WithParameterName("settings");
		}

		[Theory]
		[InlineData("TRY")]
		[InlineData("try")]
		[InlineData("Try")]
		public void IsRestricted_WithDifferentCases_ShouldReturnTrue(string currency)
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var result = validator.IsRestricted(currency);

			// Assert
			result.Should().BeTrue();
		}

		[Fact]
		public void GetDefaultBaseCurrency_WithInvalidBaseCurrency_ShouldThrowDomainException()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				DefaultBaseCurrency = "INVALID"
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.GetDefaultBaseCurrency();

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code must be exactly 3 characters");
		}

		[Fact]
		public void GetDefaultBaseCurrency_WithEmptyBaseCurrency_ShouldThrowDomainException()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				DefaultBaseCurrency = ""
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.GetDefaultBaseCurrency();

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code cannot be empty");
		}

		[Fact]
		public void GetDefaultBaseCurrency_WithNonLetterCharacters_ShouldThrowDomainException()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				DefaultBaseCurrency = "EU1"
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.GetDefaultBaseCurrency();

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code must contain only letters");
		}

		[Theory]
		[InlineData("EUR1")]
		[InlineData("E")]
		[InlineData("EURO")]
		public void IsRestricted_WithInvalidCurrencyFormat_ShouldThrowDomainException(string currency)
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.IsRestricted(currency);

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage(currency.Length != 3
					? "Currency code must be exactly 3 characters"
					: "Currency code must contain only letters");
		}

		[Fact]
		public void CheckIsRestricted_WithRestrictedCurrency_ShouldThrowDomainException()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.CheckIsRestricted("TRY");

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency TRY is restricted");
		}

		[Fact]
		public void CheckIsRestricted_WithNotRestrictedCurrency_ShouldReturnPeacefully()
		{
			// Arrange
			var settings = new CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN" }
			};
			var validator = new CurrencyValidator(settings);

			// Act
			var act = () => validator.CheckIsRestricted("EUR");

			// Assert
			act.Should().NotThrow<DomainException>();
		}
	}
}

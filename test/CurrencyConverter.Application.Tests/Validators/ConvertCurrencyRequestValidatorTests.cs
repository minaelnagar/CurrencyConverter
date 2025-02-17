using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Validators
{
	public class ConvertCurrencyRequestValidatorTests
	{
		private readonly ConvertCurrencyRequestValidator _validator;
		private readonly CurrencyValidator _currencyValidator;

		public ConvertCurrencyRequestValidatorTests()
		{
			var settings = new Domain.Common.Settings.CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};
			_currencyValidator = new CurrencyValidator(settings);
			_validator = new ConvertCurrencyRequestValidator(_currencyValidator);
		}

		[Fact]
		public async Task Validate_WithValidRequest_ShouldPass()
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = "USD",
				ToCurrency = "EUR",
				Amount = 100
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeTrue();
		}

		[Theory]
		[InlineData("")]
		[InlineData("US")]
		[InlineData("USDD")]
		public async Task Validate_WithInvalidFromCurrency_ShouldFail(string currency)
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = currency,
				ToCurrency = "EUR",
				Amount = 100
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "FromCurrency");
		}

		[Theory]
		[InlineData("TRY")]  // Restricted currency
		[InlineData("PLN")]  // Restricted currency
		public async Task Validate_WithRestrictedCurrency_ShouldFail(string currency)
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = currency,
				ToCurrency = "EUR",
				Amount = 100
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "FromCurrency");
		}

		[Theory]
		[InlineData("USD", -100)]  // Invalid amount
		[InlineData("USD", 0)]     // Invalid amount
		public async Task Validate_WithInvalidAmount_ShouldFail(string currency, decimal amount)
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = currency,
				ToCurrency = "EUR",
				Amount = amount
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "Amount");
		}
	}
}

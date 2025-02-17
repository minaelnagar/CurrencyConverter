using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Validators
{
	public class GetExchangeRateRequestValidatorTests
	{
		private readonly GetExchangeRateRequestValidator _validator;
		private readonly CurrencyValidator _currencyValidator;

		public GetExchangeRateRequestValidatorTests()
		{
			var settings = new Domain.Common.Settings.CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};
			_currencyValidator = new CurrencyValidator(settings);
			_validator = new GetExchangeRateRequestValidator(_currencyValidator);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("USD")]
		[InlineData("EUR")]
		public async Task Validate_WithValidBaseCurrency_ShouldPass(string? currency)
		{
			// Arrange
			var request = new GetExchangeRateRequest
			{
				BaseCurrency = currency
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeTrue();
		}

		[Theory]
		[InlineData("US")]
		[InlineData("USDD")]
		[InlineData("123")]
		public async Task Validate_WithInvalidBaseCurrency_ShouldFail(string currency)
		{
			// Arrange
			var request = new GetExchangeRateRequest
			{
				BaseCurrency = currency
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "BaseCurrency");
		}

		[Theory]
		[InlineData("TRY")]
		[InlineData("PLN")]
		public async Task Validate_WithRestrictedCurrency_ShouldFail(string currency)
		{
			// Arrange
			var request = new GetExchangeRateRequest
			{
				BaseCurrency = currency
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "BaseCurrency" &&
											  e.ErrorMessage.Contains("restricted"));
		}
	}
}

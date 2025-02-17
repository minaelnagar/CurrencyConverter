using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Validators
{
	public class GetHistoricalRatesRequestValidatorTests
	{
		private readonly GetHistoricalRatesRequestValidator _validator;
		private readonly CurrencyValidator _currencyValidator;

		public GetHistoricalRatesRequestValidatorTests()
		{
			var settings = new Domain.Common.Settings.CurrencySettings
			{
				RestrictedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" }
			};
			_currencyValidator = new CurrencyValidator(settings);
			_validator = new GetHistoricalRatesRequestValidator(_currencyValidator);
		}

		[Fact]
		public async Task Validate_WithValidRequest_ShouldPass()
		{
			// Arrange
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = "USD",
				StartDate = DateTime.UtcNow.AddDays(-5),
				EndDate = DateTime.UtcNow,
				Page = 1,
				PageSize = 10
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeTrue();
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public async Task Validate_WithInvalidPage_ShouldFail(int page)
		{
			// Arrange
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = "USD",
				StartDate = DateTime.UtcNow.AddDays(-5),
				EndDate = DateTime.UtcNow,
				Page = page,
				PageSize = 10
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "Page");
		}

		[Theory]
		[InlineData(0)]
		[InlineData(101)]
		public async Task Validate_WithInvalidPageSize_ShouldFail(int pageSize)
		{
			// Arrange
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = "USD",
				StartDate = DateTime.UtcNow.AddDays(-5),
				EndDate = DateTime.UtcNow,
				Page = 1,
				PageSize = pageSize
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
		}

		[Fact]
		public async Task Validate_WithFutureDate_ShouldFail()
		{
			// Arrange
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = "USD",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddDays(1),
				Page = 1,
				PageSize = 10
			};

			// Act
			var result = await _validator.ValidateAsync(request);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
		}
	}
}

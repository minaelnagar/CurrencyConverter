
namespace CurrencyConverter.Domain.Tests.Extensions
{
	public class ValidationExtensionsTests
	{
		[Theory]
		[InlineData("USD", "USD")]
		[InlineData("eur", "EUR")]
		[InlineData("GbP", "GBP")]
		public void ValidateCurrencyCode_WithValidCode_ShouldReturnUpperCase(string input, string expected)
		{
			// Act
			var result = input.ValidateCurrencyCode();

			// Assert
			result.Should().Be(expected);
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		[InlineData(" ")]
		public void ValidateCurrencyCode_WithEmptyCode_ShouldThrowDomainException(string input)
		{
			// Act
			var act = () => input.ValidateCurrencyCode();

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code cannot be empty");
		}

		[Theory]
		[InlineData("US")]
		[InlineData("USDT")]
		public void ValidateCurrencyCode_WithInvalidLength_ShouldThrowDomainException(string input)
		{
			// Act
			var act = () => input.ValidateCurrencyCode();

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code must be exactly 3 characters");
		}

		[Theory]
		[InlineData("US1")]
		[InlineData("12E")]
		[InlineData("U$D")]
		public void ValidateCurrencyCode_WithNonLetters_ShouldThrowDomainException(string input)
		{
			// Act
			var act = () => input.ValidateCurrencyCode();

			// Assert
			act.Should().Throw<DomainException>()
				.WithMessage("Currency code must contain only letters");
		}

		[Fact]
		public void ValidateCurrencyCode_WithMixedCase_ShouldReturnUpperCase()
		{
			// Arrange
			var input = "eUr";

			// Act
			var result = input.ValidateCurrencyCode();

			// Assert
			result.Should().Be("EUR");
		}
	}
}

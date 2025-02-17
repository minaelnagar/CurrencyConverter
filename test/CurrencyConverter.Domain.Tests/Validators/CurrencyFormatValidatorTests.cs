using CurrencyConverter.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Tests.Validators
{
	public class CurrencyFormatValidatorTests
	{
		[Theory]
		[InlineData("EUR")]
		[InlineData("USD")]
		[InlineData("GBP")]
		public void ValidateCurrencyCode_WithValidCode_ShouldNotThrow(string code)
		{
			var act = () => CurrencyFormatValidator.ValidateCurrencyCode(code);
			act.Should().NotThrow();
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData(null)]
		public void ValidateCurrencyCode_WithEmptyCode_ShouldThrowDomainException(string code)
		{
			var act = () => CurrencyFormatValidator.ValidateCurrencyCode(code);
			act.Should().Throw<DomainException>()
			   .WithMessage("Currency code cannot be empty");
		}

		[Theory]
		[InlineData("EU")]
		[InlineData("USDD")]
		public void ValidateCurrencyCode_WithInvalidLength_ShouldThrowDomainException(string code)
		{
			var act = () => CurrencyFormatValidator.ValidateCurrencyCode(code);
			act.Should().Throw<DomainException>()
			   .WithMessage("Currency code must be exactly 3 characters");
		}

		[Theory]
		[InlineData("EU1")]
		[InlineData("12E")]
		[InlineData("U$D")]
		public void ValidateCurrencyCode_WithNonLetters_ShouldThrowDomainException(string code)
		{
			var act = () => CurrencyFormatValidator.ValidateCurrencyCode(code);
			act.Should().Throw<DomainException>()
			   .WithMessage("Currency code must contain only letters");
		}
	}
}

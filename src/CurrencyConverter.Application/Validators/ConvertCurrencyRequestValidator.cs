using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Validators.Base;
using CurrencyConverter.Domain.Common.Extensions;
using CurrencyConverter.Domain.Services;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Application.Extensions;

namespace CurrencyConverter.Application.Validators
{
	public class ConvertCurrencyRequestValidator :CurrencyValidatorBase<ConvertCurrencyRequest>
	{
		public ConvertCurrencyRequestValidator(CurrencyValidator currencyValidator)
			: base(currencyValidator)
		{
			RuleFor(x => x.FromCurrency)
				.NotEmpty().WithMessage("From currency is required")
				.Must(BeValidCurrency).WithMessage("From currency has invalid format")
				.Must(NotBeRestricted).WithMessage("From currency is restricted");

			RuleFor(x => x.ToCurrency)
				.NotEmpty().WithMessage("To currency is required")
				.Must(BeValidCurrency).WithMessage("To currency has invalid format")
				.Must(NotBeRestricted).WithMessage("To currency is restricted");

			RuleFor(x => x.Amount)
				.GreaterThan(0).WithMessage("Amount must be greater than zero");
		}
	}
}

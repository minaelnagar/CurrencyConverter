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

namespace CurrencyConverter.Application.Validators
{
	public class GetExchangeRateRequestValidator :CurrencyValidatorBase<GetExchangeRateRequest>
	{
		public GetExchangeRateRequestValidator(CurrencyValidator currencyValidator)
			: base(currencyValidator)
		{
			RuleFor(x => x.BaseCurrency)
				.Must(BeValidCurrencyOrNull).WithMessage("Base currency has invalid format")
				.Must(NotBeRestrictedOrNull).WithMessage("Base currency is restricted");
		}

		private bool BeValidCurrencyOrNull(string? currency)
		{
			if(string.IsNullOrEmpty(currency))
				return true;

			return BeValidCurrency(currency);
		}

		private bool NotBeRestrictedOrNull(string? currency)
		{
			if(string.IsNullOrEmpty(currency))
				return true;

			return NotBeRestricted(currency);
		}
	}
}

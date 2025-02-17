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
	public class GetHistoricalRatesRequestValidator :CurrencyValidatorBase<GetHistoricalRatesRequest>
	{
		public GetHistoricalRatesRequestValidator(CurrencyValidator currencyValidator)
			: base(currencyValidator)
		{
			RuleFor(x => x.BaseCurrency)
				.NotEmpty().WithMessage("Base currency is required")
				.Must(BeValidCurrency).WithMessage("Base currency has invalid format")
				.Must(NotBeRestricted).WithMessage("Base currency is restricted");

			RuleFor(x => x.StartDate)
				.NotEmpty().WithMessage("Start date is required")
				.LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date")
				.Must(BeValidDate).WithMessage("Start date cannot be in the future");

			RuleFor(x => x.EndDate)
				.NotEmpty().WithMessage("End date is required")
				.Must(BeValidDate).WithMessage("End date cannot be in the future");

			RuleFor(x => x.PageSize)
				.InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

			RuleFor(x => x.Page)
				.GreaterThan(0).WithMessage("Page number must be greater than 0");
		}
	}
}

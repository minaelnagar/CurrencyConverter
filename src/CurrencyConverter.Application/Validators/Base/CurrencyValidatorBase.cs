using CurrencyConverter.Domain.Common.Extensions;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Services;
using CurrencyConverter.Domain.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Validators.Base
{
	public abstract class CurrencyValidatorBase<T> :AbstractValidator<T>
	{
		protected readonly CurrencyValidator CurrencyValidator;

		protected CurrencyValidatorBase(CurrencyValidator currencyValidator)
		{
			CurrencyValidator = currencyValidator;
		}

		protected bool BeValidCurrency(string currency)
		{
			try
			{
				CurrencyFormatValidator.ValidateCurrencyCode(currency);
				return true;
			}
			catch(DomainException)
			{
				return false;
			}
		}

		protected bool NotBeRestricted(string currency)
		{
			try
			{
				return !CurrencyValidator.IsRestricted(currency);
			}
			catch(DomainException)
			{
				return true;
			}
		}

		protected bool BeValidDate(DateTime date)
		{
			return date.Date <= DateTime.UtcNow.Date;
		}
	}

}

using CurrencyConverter.Domain.Common.Extensions;
using CurrencyConverter.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Validators
{
	public static class CurrencyFormatValidator
	{
		public static void ValidateCurrencyCode(string currencyCode)
		{
			if(string.IsNullOrWhiteSpace(currencyCode))
				throw new DomainException("Currency code cannot be empty");

			if(currencyCode.Length != 3)
				throw new DomainException("Currency code must be exactly 3 characters");

			if(!currencyCode.All(char.IsLetter))
				throw new DomainException("Currency code must contain only letters");
		}
	}
}

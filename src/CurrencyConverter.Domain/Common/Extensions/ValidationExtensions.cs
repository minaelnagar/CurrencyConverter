using CurrencyConverter.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Common.Extensions
{
	public static class ValidationExtensions
	{
		public static string ValidateCurrencyCode(this string currencyCode)
		{
			if(string.IsNullOrWhiteSpace(currencyCode))
				throw new DomainException("Currency code cannot be empty");

			if(currencyCode.Length != 3)
				throw new DomainException("Currency code must be exactly 3 characters");

			if(!currencyCode.All(char.IsLetter))
				throw new DomainException("Currency code must contain only letters");

			return currencyCode.ToUpperInvariant();
		}
	}
}

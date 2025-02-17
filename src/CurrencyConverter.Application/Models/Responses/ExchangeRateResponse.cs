using CurrencyConverter.Domain.Common.Extensions;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Models.Responses
{
	public record ExchangeRateResponse
	{
		public required string BaseCurrency { get; init; }
		public required DateTime Date { get; init; }
		public required Dictionary<string, decimal> Rates { get; init; }

		public decimal? GetRate(string currencyCode, CurrencyValidator currencyValidator)
		{
			var validCurrency = currencyCode.ValidateCurrencyCode();

			currencyValidator.CheckIsRestricted(validCurrency);	

			return Rates.TryGetValue(validCurrency, out var rate) ? rate : null;
		}

		public bool HasRate(string currencyCode)
		{
			var validCurrency = currencyCode.ValidateCurrencyCode();
			return Rates.ContainsKey(validCurrency);
		}
	}
}

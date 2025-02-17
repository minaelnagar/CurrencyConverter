using CurrencyConverter.Domain.Common.Extensions;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Entities
{
	public class ExchangeRate
	{
		public Guid Id { get; private set; }
		public string BaseCurrency { get; private set; }
		public DateTime Date { get; private set; }
		public Dictionary<string, decimal> Rates { get; private set; }

		[JsonConstructor]
		private ExchangeRate(string baseCurrency, DateTime date, Dictionary<string, decimal> rates)
		{
			Id = Guid.NewGuid();
			BaseCurrency = baseCurrency.ValidateCurrencyCode();
			Date = date;
			Rates = rates;
		}

		public static ExchangeRate Create(
			string baseCurrency,
			DateTime date,
			Dictionary<string, decimal> rates,
			CurrencyValidator currencyValidator)
		{
			// Validate base currency
			baseCurrency = baseCurrency.ValidateCurrencyCode();
			if(currencyValidator.IsRestricted(baseCurrency))
				throw new DomainException($"Currency {baseCurrency} is restricted");

			if(rates == null || !rates.Any())
				throw new DomainException("Rates cannot be empty");

			// Validate and clean rates
			var validRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
			foreach(var (currency, rate) in rates)
			{
				var validCurrency = currency.ValidateCurrencyCode();
				if(!currencyValidator.IsRestricted(validCurrency))
				{
					if(rate <= 0)
						throw new DomainException($"Rate for {validCurrency} must be greater than zero");

					validRates[validCurrency] = rate;
				}
			}

			return new ExchangeRate(baseCurrency, date, validRates);
		}
	}
}

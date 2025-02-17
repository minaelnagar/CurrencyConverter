using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.TestHelpers
{
	public static class ExchangeRateTestHelper
	{
		public static ExchangeRate CreateExchangeRate(
			string baseCurrency,
			Dictionary<string, decimal>? rates = null)
		{
			return ExchangeRate.Create(
				baseCurrency,
				DateTime.UtcNow,
				rates ?? new Dictionary<string, decimal>
				{
					["EUR"] = 0.85m,
					["GBP"] = 0.73m
				},
				new CurrencyValidator(new Domain.Common.Settings.CurrencySettings()));
		}
	}
}

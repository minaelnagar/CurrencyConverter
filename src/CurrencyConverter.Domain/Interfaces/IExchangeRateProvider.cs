using CurrencyConverter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Interfaces
{
	public interface IExchangeRateProvider
	{
		Task<ExchangeRate> GetLatestRatesAsync(
		string baseCurrency,
		CancellationToken cancellationToken = default);

		Task<ExchangeRate> GetHistoricalRatesAsync(
			string baseCurrency,
			DateTime date,
			CancellationToken cancellationToken = default);

		Task<IEnumerable<ExchangeRate>> GetHistoricalRatesRangeAsync(
			string baseCurrency,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken = default);
	}
}

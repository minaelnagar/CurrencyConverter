using CurrencyConverter.Application.Models.Responses;
using CurrencyConverter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Interfaces
{
	public interface IExchangeRateService
	{
		Task<ExchangeRateResponse> GetLatestRatesAsync(
			string? baseCurrency = null,
			CancellationToken cancellationToken = default);

		Task<ExchangeRateResponse> GetHistoricalRatesAsync(
			string baseCurrency,
			DateTime date,
			CancellationToken cancellationToken = default);

		Task<PagedResponse<ExchangeRateResponse>> GetHistoricalRatesRangeAsync(
			string baseCurrency,
			DateTime startDate,
			DateTime endDate,
			int page = 1,
			int pageSize = 10,
			CancellationToken cancellationToken = default);
	}
}

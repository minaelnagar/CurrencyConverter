using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Services;
using CurrencyConverter.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.ExternalServices
{
	public class FrankfurterApiClient :IExchangeRateProvider
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<FrankfurterApiClient> _logger;
		private readonly CurrencyValidator _currencyValidator;

		public FrankfurterApiClient(
			HttpClient httpClient,
			ILogger<FrankfurterApiClient> logger,
			CurrencyValidator currencyValidator)
		{
			_httpClient = httpClient;
			_logger = logger;
			_currencyValidator = currencyValidator;

			_httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
		}

		public async Task<ExchangeRate> GetLatestRatesAsync(
			string baseCurrency,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.GetAsync(
					$"latest?base={baseCurrency}",
					cancellationToken);

				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadFromJsonAsync<FrankfurterResponse>(
					cancellationToken: cancellationToken);

				return ExchangeRate.Create(
					content.Base,
					content.Date,
					content.Rates,
					_currencyValidator);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error getting latest rates for base currency: {Currency}", baseCurrency);
				throw;
			}
		}

		public async Task<ExchangeRate> GetHistoricalRatesAsync(
	  string baseCurrency,
	  DateTime date,
	  CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.GetAsync(
					$"{date:yyyy-MM-dd}?base={baseCurrency}",
					cancellationToken);

				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadFromJsonAsync<FrankfurterResponse>(
					cancellationToken: cancellationToken);

				return ExchangeRate.Create(
					content.Base,
					content.Date,
					content.Rates,
					_currencyValidator);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error getting historical rates for base currency: {Currency} on date: {Date}",
					baseCurrency, date);
				throw;
			}
		}

		public async Task<IEnumerable<ExchangeRate>> GetHistoricalRatesRangeAsync(
			string baseCurrency,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.GetAsync(
					$"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}",
					cancellationToken);

				response.EnsureSuccessStatusCode();

				//Read to string then parse to dictionary

				var content = await response.Content.ReadFromJsonAsync<FrankfurterTimeSeriesResponse>(
					cancellationToken: cancellationToken);

				return content.Rates.Select(kvp => ExchangeRate.Create(
					content.Base,
					kvp.Key,
					kvp.Value,
					_currencyValidator));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Error getting historical rates range for base currency: {Currency} from {StartDate} to {EndDate}",
					baseCurrency, startDate, endDate);
				throw;
			}
		}

	}
}

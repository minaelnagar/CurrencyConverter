using CurrencyConverter.Application.Models.Responses;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace CurrencyConverter.API.Examples
{
	[ExcludeFromCodeCoverage]
	public class LatestRatesExample :IExamplesProvider<ExchangeRateResponse>
	{
		public ExchangeRateResponse GetExamples()
		{
			return new ExchangeRateResponse
			{
				BaseCurrency = "EUR",
				Date = DateTime.UtcNow,
				Rates = new Dictionary<string, decimal>
				{
					["USD"] = 1.18m,
					["GBP"] = 0.86m,
					["JPY"] = 129.50m,
					["CHF"] = 1.08m,
					["AUD"] = 1.62m
				}
			};
		}
	}
	[ExcludeFromCodeCoverage]
	public class CurrencyConversionExample :IExamplesProvider<CurrencyConversionResponse>
	{
		public CurrencyConversionResponse GetExamples()
		{
			return new CurrencyConversionResponse
			{
				FromCurrency = "USD",
				ToCurrency = "EUR",
				Amount = 100.00m,
				ConvertedAmount = 85.00m,
				Rate = 0.85m
			};
		}
	}
	[ExcludeFromCodeCoverage]
	public class HistoricalRatesExample :IExamplesProvider<PagedResponse<ExchangeRateResponse>>
	{
		public PagedResponse<ExchangeRateResponse> GetExamples()
		{
			return new PagedResponse<ExchangeRateResponse>
			{
				Items = new List<ExchangeRateResponse>
				{
					new()
					{
						BaseCurrency = "EUR",
						Date = DateTime.UtcNow.AddDays(-2),
						Rates = new Dictionary<string, decimal>
						{
							["USD"] = 1.18m,
							["GBP"] = 0.86m
						}
					},
					new()
					{
						BaseCurrency = "EUR",
						Date = DateTime.UtcNow.AddDays(-1),
						Rates = new Dictionary<string, decimal>
						{
							["USD"] = 1.19m,
							["GBP"] = 0.87m
						}
					}
				},
				CurrentPage = 1,
				PageSize = 10,
				TotalItems = 31,
				TotalPages = 4
			};
		}
	}

}

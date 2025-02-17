using CurrencyConverter.Application.Infrastructure.Abstractions;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Models.Responses;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Services
{
	public class ExchangeRateService :IExchangeRateService
	{
		private readonly IValidator<GetExchangeRateRequest> _exchangeRateValidator;
		private readonly IValidator<GetHistoricalRatesRequest> _historicalRatesValidator;
		private readonly ICacheService _cacheService;
		private readonly IExchangeRateProvider _exchangeRateProvider;
		private readonly CurrencyValidator _currencyValidator;
		private readonly ILogger<ExchangeRateService> _logger;

		public ExchangeRateService(
			IValidator<GetExchangeRateRequest> exchangeRateValidator,
			IValidator<GetHistoricalRatesRequest> historicalRatesValidator,
			ICacheService cacheService,
			IExchangeRateProvider exchangeRateProvider,
			CurrencyValidator currencyValidator,
			ILogger<ExchangeRateService> logger)
		{
			_exchangeRateValidator = exchangeRateValidator;
			_historicalRatesValidator = historicalRatesValidator;
			_cacheService = cacheService;
			_exchangeRateProvider = exchangeRateProvider;
			_currencyValidator = currencyValidator;
			_logger = logger;
		}

		public async Task<ExchangeRateResponse> GetLatestRatesAsync(
			string? baseCurrency = null,
			CancellationToken cancellationToken = default)
		{
			var request = new GetExchangeRateRequest { BaseCurrency = baseCurrency };

			var validationResult = await _exchangeRateValidator.ValidateAsync(request, cancellationToken);
			if(!validationResult.IsValid)
			{
				throw new Application.Exceptions.ValidationException(validationResult.Errors);
			}

			var currency = baseCurrency ?? _currencyValidator.GetDefaultBaseCurrency();
			var cacheKey = $"rates:{currency}:latest";

			var rates = await _cacheService.GetAsync<ExchangeRate>(cacheKey, cancellationToken);
			if(rates != null)
			{
				_logger.LogInformation("Retrieved latest rates for {Currency} from cache", currency);
			}
			else
			{
				rates = await _exchangeRateProvider.GetLatestRatesAsync(currency, cancellationToken);

				await _cacheService.SetAsync(
					cacheKey,
					rates,
					TimeSpan.FromMinutes(5),
					cancellationToken);

				_logger.LogInformation("Retrieved and cached latest rates for {Currency}", currency);
			}

			return new ExchangeRateResponse
			{
				BaseCurrency = rates.BaseCurrency,
				Date = rates.Date,
				Rates = rates.Rates
			};
		}

		public async Task<ExchangeRateResponse> GetHistoricalRatesAsync(
		string baseCurrency,
		DateTime date,
		CancellationToken cancellationToken = default)
		{
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = baseCurrency,
				StartDate = date,
				EndDate = date,
				Page = 1,
				PageSize = 1
			};

			var validationResult = await _historicalRatesValidator.ValidateAsync(request, cancellationToken);
			if(!validationResult.IsValid)
			{
				throw new Application.Exceptions.ValidationException(validationResult.Errors);
			}

			var cacheKey = $"rates:{baseCurrency}:{date:yyyy-MM-dd}";
			var rates = await _cacheService.GetAsync<ExchangeRate>(cacheKey, cancellationToken);

			if(rates != null)
			{
				_logger.LogInformation(
					"Retrieved historical rates for {Currency} on {Date} from cache",
					baseCurrency,
					date);
			}
			else
			{

				rates = await _exchangeRateProvider.GetHistoricalRatesAsync(
					baseCurrency,
					date,
					cancellationToken);

				await _cacheService.SetAsync(
					cacheKey,
					rates,
					TimeSpan.FromDays(1),
					cancellationToken);


				_logger.LogInformation(
					"Retrieved and cached historical rates for {Currency} on {Date}",
					baseCurrency,
					date);
			}

			return new ExchangeRateResponse
			{
				BaseCurrency = rates.BaseCurrency,
				Date = rates.Date,
				Rates = rates.Rates
			};
		}


		public async Task<PagedResponse<ExchangeRateResponse>> GetHistoricalRatesRangeAsync(
		   string baseCurrency,
		   DateTime startDate,
		   DateTime endDate,
		   int page,
		   int pageSize,
		   CancellationToken cancellationToken = default)
		{
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = baseCurrency,
				StartDate = startDate,
				EndDate = endDate,
				Page = page,
				PageSize = pageSize
			};

			var validationResult = await _historicalRatesValidator.ValidateAsync(request, cancellationToken);
			if(!validationResult.IsValid)
			{
				throw new Application.Exceptions.ValidationException(validationResult.Errors);
			}

			var totalDays = (endDate - startDate).Days + 1;
			var skip = (page - 1) * pageSize;
			var currentPageStartDate = startDate.AddDays(skip);
			var currentPageEndDate = currentPageStartDate.AddDays(pageSize - 1);

			if(currentPageEndDate > endDate)
				currentPageEndDate = endDate;

			var cacheKey = $"rates:{baseCurrency}:{currentPageStartDate:yyyy-MM-dd}:{currentPageEndDate:yyyy-MM-dd}";
			var rates = await _cacheService.GetAsync<IEnumerable<ExchangeRate>>(
				cacheKey,
				cancellationToken);

			if(rates == null)
			{
				rates = await _exchangeRateProvider.GetHistoricalRatesRangeAsync(
					baseCurrency,
					currentPageStartDate,
					currentPageEndDate,
					cancellationToken);

				await _cacheService.SetAsync(
					cacheKey,
					rates,
					TimeSpan.FromDays(1),
					cancellationToken);

				_logger.LogInformation(
					"Retrieved and cached historical rates for {Currency} from {StartDate} to {EndDate}",
					baseCurrency,
					currentPageStartDate,
					currentPageEndDate);
			}
			else
			{
				_logger.LogInformation(
					"Retrieved historical rates from cache for {Currency} from {StartDate} to {EndDate}",
					baseCurrency,
					currentPageStartDate,
					currentPageEndDate);
			}

			var items = rates.Select(rate => new ExchangeRateResponse
			{
				BaseCurrency = rate.BaseCurrency,
				Date = rate.Date,
				Rates = rate.Rates
			});

			return new PagedResponse<ExchangeRateResponse>
			{
				Items = items.ToList(),
				CurrentPage = page,
				PageSize = pageSize,
				TotalItems = totalDays,
				TotalPages = (int)Math.Ceiling(totalDays / (double)pageSize)
			};
		}
	}
}
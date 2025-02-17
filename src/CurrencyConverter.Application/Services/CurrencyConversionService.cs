using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Models.Responses;
using CurrencyConverter.Domain.Exceptions;
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
	public class CurrencyConversionService :ICurrencyConversionService
	{
		private readonly IValidator<ConvertCurrencyRequest> _validator;
		private readonly IExchangeRateService _exchangeRateService;
		private readonly CurrencyValidator _currencyValidator;
		private readonly ILogger<CurrencyConversionService> _logger;

		public CurrencyConversionService(
			IValidator<ConvertCurrencyRequest> validator,
			IExchangeRateService exchangeRateService,
			CurrencyValidator currencyValidator,
			ILogger<CurrencyConversionService> logger)
		{
			_validator = validator;
			_exchangeRateService = exchangeRateService;
			_currencyValidator = currencyValidator;
			_logger = logger;
		}

		public async Task<CurrencyConversionResponse> ConvertAsync(
			ConvertCurrencyRequest request,
			CancellationToken cancellationToken = default)
		{
			var validationResult = await _validator.ValidateAsync(request, cancellationToken);
			if(!validationResult.IsValid)
			{
				throw new Application.Exceptions.ValidationException(validationResult.Errors);
			}

			try
			{
				var rates = await _exchangeRateService.GetLatestRatesAsync(
					request.FromCurrency,
					cancellationToken);

				var rate = rates.GetRate(request.ToCurrency, _currencyValidator)
					?? throw new DomainException($"No rate found for {request.ToCurrency}");

				var convertedAmount = request.Amount * rate;

				_logger.LogInformation(
					"Converted {Amount} {FromCurrency} to {ConvertedAmount} {ToCurrency}",
					request.Amount,
					request.FromCurrency,
					convertedAmount,
					request.ToCurrency);

				return new CurrencyConversionResponse
				{
					FromCurrency = request.FromCurrency,
					ToCurrency = request.ToCurrency,
					Amount = request.Amount,
					ConvertedAmount = convertedAmount,
					Rate = rate
				};
			}
			catch(DomainException)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Error converting {Amount} {FromCurrency} to {ToCurrency}",
					request.Amount,
					request.FromCurrency,
					request.ToCurrency);
				throw;
			}
		}
	}
}

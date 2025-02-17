using CurrencyConverter.API.Models;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.API.Controllers.V1
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	[Authorize]
	[SwaggerTag("Manages currency exchange rates and conversions")]

	public class ExchangeRateController :ControllerBase
	{
		private readonly IExchangeRateService _exchangeRateService;
		private readonly ICurrencyConversionService _currencyConversionService;
		private readonly ILogger<ExchangeRateController> _logger;

		public ExchangeRateController(
			IExchangeRateService exchangeRateService, ICurrencyConversionService currencyConversionService, ILogger<ExchangeRateController> logger)
		{
			_exchangeRateService = exchangeRateService;
			_currencyConversionService = currencyConversionService;
			_logger = logger;
		}

		/// <summary>
		/// Retrieves the latest exchange rates for a specified base currency
		/// </summary>
		/// <remarks>
		/// Common error scenarios:
		/// * Invalid currency code (not 3 letters)
		/// * Restricted currency used (TRY, PLN, THB, MXN)
		/// * Rate limit exceeded (too many requests)
		/// * External API failure (temporary unavailable)
		/// 
		/// Sample error responses:
		/// 
		/// 1. Validation Error (400):
		///     {
		///         "code": "ValidationError",
		///         "messages": ["Invalid currency code format"]
		///     }
		/// 
		/// 2. Restricted Currency (400):
		///     {
		///         "code": "RestrictedCurrency",
		///         "messages": ["Currency TRY is restricted"]
		///     }
		/// 
		/// 3. Rate Limit (429):
		///     {
		///         "code": "RateLimitExceeded",
		///         "messages": ["Rate limit exceeded. Try again in 60 seconds."]
		///     }
		/// 
		/// 4. Service Unavailable (503):
		///     {
		///         "code": "ServiceUnavailable",
		///         "messages": ["Exchange rate service is temporarily unavailable"]
		///     }
		/// </remarks>
		[HttpGet("latest")]
		[Authorize(Roles = "User")]
		[ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<ExchangeRateResponse>> GetLatestRates(
			[FromQuery] string? baseCurrency = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var rates = await _exchangeRateService.GetLatestRatesAsync(baseCurrency, cancellationToken);
				return Ok(rates);
			}
			catch(Application.Exceptions.ValidationException ex)
			{
				return BadRequest(ex.Errors);
			}
			catch(Exception)
			{

				throw;
			}
		}


		/// <summary>
		/// Converts an amount from one currency to another
		/// </summary>
		/// <param name="request">Currency conversion request details</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <remarks>
		/// Sample request:
		/// 
		///     POST /api/v1/exchangerate/convert
		///     {
		///         "fromCurrency": "USD",
		///         "toCurrency": "EUR",
		///         "amount": 100.00
		///     }
		/// 
		/// Sample response:
		/// 
		///     {
		///         "fromCurrency": "USD",
		///         "toCurrency": "EUR",
		///         "amount": 100.00,
		///         "convertedAmount": 85.00,
		///         "rate": 0.85
		///     }
		/// </remarks>
		/// <returns>Conversion result with rate used</returns>
		/// <response code="200">Returns the conversion result</response>
		/// <response code="400">If currencies are invalid or restricted</response>
		/// <response code="401">If the user is not authenticated</response>
		/// <response code="429">If rate limit is exceeded</response>
		[HttpPost("convert")]
		[Authorize(Roles = "User")]
		[ProducesResponseType(typeof(CurrencyConversionResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<CurrencyConversionResponse>> ConvertCurrency(
			[FromBody] ConvertCurrencyRequest request,
			CancellationToken cancellationToken = default)
		{
			var result = await _currencyConversionService.ConvertAsync(request, cancellationToken);
			return Ok(result);
		}

		/// <summary>
		/// Retrieves historical exchange rates for a given period
		/// </summary>
		/// <param name="request">Historical rates request parameters</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <remarks>
		/// Sample request:
		/// 
		///     GET /api/v1/exchangerate/historical?baseCurrency=EUR&amp;startDate=2024-01-01&amp;endDate=2024-01-31&amp;page=1&amp;pageSize=10
		/// 
		/// Sample response:
		/// 
		///     {
		///         "items": [
		///             {
		///                 "baseCurrency": "EUR",
		///                 "date": "2024-01-01T00:00:00Z",
		///                 "rates": {
		///                     "USD": 1.18,
		///                     "GBP": 0.86,
		///                     "JPY": 129.50
		///                 }
		///             }
		///         ],
		///         "totalItems": 31,
		///         "page": 1,
		///         "pageSize": 10,
		///         "totalPages": 4
		///     }
		/// </remarks>
		[HttpGet("historical")]
		[Authorize(Roles = "User")]
		[ProducesResponseType(typeof(PagedResponse<ExchangeRateResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<PagedResponse<ExchangeRateResponse>>> GetHistoricalRates(
			[FromQuery] GetHistoricalRatesRequest request,
			CancellationToken cancellationToken = default)
		{
			var rates = await _exchangeRateService.GetHistoricalRatesRangeAsync(
				request.BaseCurrency,
				request.StartDate,
				request.EndDate,
				request.Page,
				request.PageSize,
				cancellationToken);
			return Ok(rates);
		}
	}
}

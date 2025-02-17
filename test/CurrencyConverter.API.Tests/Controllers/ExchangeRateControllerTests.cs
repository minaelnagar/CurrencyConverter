using CurrencyConverter.API.Controllers.V1;
using CurrencyConverter.Application.Exceptions;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Models.Responses;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.TestHelpers;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.API.Tests.Controllers
{
	public class ExchangeRateControllerTests
	{
		private readonly Mock<IExchangeRateService> _service;
		private readonly Mock<ICurrencyConversionService> _currencyConversionService;
		private readonly Mock<ILogger<ExchangeRateController>> _logger;
		private readonly ExchangeRateController _controller;

		public ExchangeRateControllerTests()
		{
			_service = new Mock<IExchangeRateService>();
			_currencyConversionService = new Mock<ICurrencyConversionService>();
			_logger = new Mock<ILogger<ExchangeRateController>>();
			_controller = new ExchangeRateController(_service.Object, _currencyConversionService.Object, _logger.Object);
		}

		[Fact]
		public async Task GetLatestRates_WithValidRequest_ReturnsOkResult()
		{
			// Arrange
			var expectedRates = new ExchangeRateResponse
			{
				BaseCurrency = "EUR",
				Date = DateTime.UtcNow,
				Rates = new Dictionary<string, decimal> { ["USD"] = 1.1m }
			};
			
			_service.Setup(x => x.GetLatestRatesAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedRates);

			// Act
			var result = await _controller.GetLatestRates();

			// Assert
			var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
			var response = okResult.Value.Should().BeOfType<ExchangeRateResponse>().Subject;
			response.Should().BeEquivalentTo(expectedRates);
		}

		[Fact]
		public async Task GetLatestRates_WhenValidationFails_ReturnsBadRequest()
		{
			// Arrange
			_service.Setup(x => x.GetLatestRatesAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new ValidationException(new[] { new ValidationFailure("BaseCurrency", "Invalid") }));

			// Act
			var result = await _controller.GetLatestRates("INVALID");

			// Assert
			result.Result.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task GetLatestRates_WhenServiceThrowsNonHandledException_ThrowException()
		{
			// Arrange
			_service.Setup(x => x.GetLatestRatesAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("TESTING"));

			// Act
			var act = () => _controller.GetLatestRates("INVALID");

			// Assert
			await act.Should().ThrowAsync<Exception>();
		}

		[Fact]
		public async Task ConvertCurrency_WithValidRequest_ReturnsOkResult()
		{
			// Arrange
			var request = new ConvertCurrencyRequest
			{
				FromCurrency = "USD",
				ToCurrency = "EUR",
				Amount = 100
			};

			var expectedResult = new CurrencyConversionResponse
			{
				FromCurrency = "USD",
				ToCurrency = "EUR",
				Amount = 100,
				ConvertedAmount = 85,
				Rate = 0.85m
			};

			_currencyConversionService.Setup(x => x.ConvertAsync(
				It.IsAny<ConvertCurrencyRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedResult);

			// Act
			var result = await _controller.ConvertCurrency(request);

			// Assert
			var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
			var response = okResult.Value.Should().BeOfType<CurrencyConversionResponse>().Subject;
			response.Should().BeEquivalentTo(expectedResult);
		}

		[Fact]
		public async Task GetHistoricalRates_WithValidRequest_ReturnsOkResult()
		{
			// Arrange
			var request = new GetHistoricalRatesRequest
			{
				BaseCurrency = "EUR",
				StartDate = DateTime.UtcNow.AddDays(-5),
				EndDate = DateTime.UtcNow,
				Page = 1,
				PageSize = 10
			};

			var expectedResult = new PagedResponse<ExchangeRateResponse>
			{
				Items = new List<ExchangeRateResponse>() { 
					new ExchangeRateResponse
					{
						BaseCurrency = "EUR",
						Date = DateTime.UtcNow,
						Rates = new Dictionary<string, decimal> { ["USD"] = 1.1m }
					}},
				CurrentPage = 1,
				PageSize = 10,
				TotalItems = 1,
				TotalPages = 1
			};

			_service.Setup(x => x.GetHistoricalRatesRangeAsync(
				It.IsAny<string>(),
				It.IsAny<DateTime>(),
				It.IsAny<DateTime>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedResult);

			// Act
			var result = await _controller.GetHistoricalRates(request);

			// Assert
			var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
			var response = okResult.Value.Should().BeOfType<PagedResponse<ExchangeRateResponse>>().Subject;
			response.Should().BeEquivalentTo(expectedResult);
		}
	}
}

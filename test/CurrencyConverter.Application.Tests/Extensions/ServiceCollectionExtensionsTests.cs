using CurrencyConverter.Application.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Extensions
{
	public class ServiceCollectionExtensionsTests
	{
		private readonly ServiceCollection _services;
		private readonly IConfiguration _configuration;

		public ServiceCollectionExtensionsTests()
		{
			_services = new ServiceCollection();

			var inMemorySettings = new List<KeyValuePair<string, string?>>()
			{
				new KeyValuePair<string,string?>( "CurrencySettings:DefaultBaseCurrency", "EUR" ),
				new KeyValuePair < string, string ? >("CurrencySettings:RestrictedCurrencies:0", "TRY"),
				new KeyValuePair < string, string ? >("CurrencySettings:RestrictedCurrencies:1", "PLN"),
				new KeyValuePair < string, string ? >("CurrencySettings:RestrictedCurrencies:2", "THB"),
				new KeyValuePair < string, string ? >("CurrencySettings:RestrictedCurrencies:3", "MXN")
			};

			_configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(inMemorySettings)
				.Build();
		}

		[Fact]
		public void AddApplicationValidators_ReturnsSameServiceCollection()
		{
			// Arrange

			// Act
			var returnedServices = _services.AddApplicationServices(_configuration);

			// Assert
			Assert.Equal(_services, returnedServices);
		}

		[Fact]
		public void AddApplicationValidators_RegistersGetExchangeRateRequestValidator()
		{
			// Arrange
			_services.AddApplicationServices(_configuration);
			var provider = _services.BuildServiceProvider();

			// Act
			var validator = provider.GetService<IValidator<GetExchangeRateRequest>>();

			// Assert
			Assert.NotNull(validator);
			Assert.IsType<GetExchangeRateRequestValidator>(validator);
		}

		[Fact]
		public void AddApplicationValidators_RegistersConvertCurrencyRequestValidator()
		{
			// Arrange
			_services.AddApplicationServices(_configuration);
			var provider = _services.BuildServiceProvider();

			// Act
			var validator = provider.GetService<IValidator<ConvertCurrencyRequest>>();

			// Assert
			Assert.NotNull(validator);
			Assert.IsType<ConvertCurrencyRequestValidator>(validator);
		}

		[Fact]
		public void AddApplicationValidators_RegistersGetHistoricalRatesRequestValidator()
		{
			// Arrange
			_services.AddApplicationServices(_configuration);
			var provider = _services.BuildServiceProvider();

			// Act
			var validator = provider.GetService<IValidator<GetHistoricalRatesRequest>>();

			// Assert
			Assert.NotNull(validator);
			Assert.IsType<GetHistoricalRatesRequestValidator>(validator);
		}
	}
}

using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Application.Validators;
using CurrencyConverter.Domain.Common.Settings;
using CurrencyConverter.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplicationServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddOptions<CurrencySettings>()
				.Bind(configuration.GetSection("CurrencySettings"))
				.ValidateDataAnnotations();

			services.AddSingleton<CurrencySettings>(sp => sp.GetRequiredService<IOptions<CurrencySettings>>().Value);

			services.AddScoped<CurrencyValidator>();
			services.AddScoped<IValidator<GetExchangeRateRequest>, GetExchangeRateRequestValidator>();
			services.AddScoped<IValidator<ConvertCurrencyRequest>, ConvertCurrencyRequestValidator>();
			services.AddScoped<IValidator<GetHistoricalRatesRequest>, GetHistoricalRatesRequestValidator>();

			services.AddScoped<IExchangeRateService, ExchangeRateService>();
			services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();


			return services;
		}
	}
}

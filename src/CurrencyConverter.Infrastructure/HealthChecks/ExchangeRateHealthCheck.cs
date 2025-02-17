using CurrencyConverter.Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.HealthChecks
{
	public class ExchangeRateHealthCheck :IHealthCheck
	{
		private readonly IExchangeRateProvider _exchangeRateProvider;
		private readonly ILogger<ExchangeRateHealthCheck> _logger;

		public ExchangeRateHealthCheck(
			IExchangeRateProvider exchangeRateProvider,
			ILogger<ExchangeRateHealthCheck> logger)
		{
			_exchangeRateProvider = exchangeRateProvider;
			_logger = logger;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await _exchangeRateProvider.GetLatestRatesAsync("EUR", cancellationToken);
				return HealthCheckResult.Healthy();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Health check failed");
				return HealthCheckResult.Unhealthy(ex.Message);
			}
		}
	}

}

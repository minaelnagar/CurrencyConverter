using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Logging
{
	public static class LoggerConfigurationExtensions
	{
		public static LoggerConfiguration AddCustomEnrichers(this LoggerConfiguration configuration)
		{
			return configuration
				.Enrich.WithThreadId()
				.Enrich.WithEnvironmentName()
				.Enrich.WithMachineName()
				.Enrich.FromLogContext();
		}
	}

}

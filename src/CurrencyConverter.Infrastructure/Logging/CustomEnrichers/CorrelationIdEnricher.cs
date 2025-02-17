using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Logging.CustomEnrichers
{
	public class CorrelationIdEnricher :ILogEventEnricher
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
			var property = propertyFactory.CreateProperty("CorrelationId", correlationId ?? "N/A");
			logEvent.AddPropertyIfAbsent(property);
		}
	}
}

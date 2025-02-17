using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Logging
{
	public class InMemorySink :ILogEventSink
	{
		private readonly IList<LogEvent> _logEvents;
		public InMemorySink(IList<LogEvent> logEvents) => _logEvents = logEvents;
		public void Emit(LogEvent logEvent) => _logEvents.Add(logEvent);
	}
}

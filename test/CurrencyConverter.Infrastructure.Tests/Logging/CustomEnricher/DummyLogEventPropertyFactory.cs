using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Logging.CustomEnricher
{
	public class DummyLogEventPropertyFactory :Serilog.Core.ILogEventPropertyFactory
	{
		public LogEventProperty CreateProperty(string name, object value, bool destructureObjects = false)
		{
			return new LogEventProperty(name, new ScalarValue(value));
		}
	}

}

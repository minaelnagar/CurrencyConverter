using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Infrastructure.Logging;

namespace CurrencyConverter.Infrastructure.Tests.Logging
{
	public class LoggerConfigurationExtensionsTests
	{
		[Fact]
		public void AddCustomEnrichers_AddsExpectedEnrichers()
		{
			// Arrange
			var logEvents = new List<LogEvent>();
			var sink = new InMemorySink(logEvents);

			Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");


			// Create a logger configuration and apply the custom enrichers.
			// The extension method under test is AddCustomEnrichers.
			var logger = new LoggerConfiguration()
				.AddCustomEnrichers()
				.WriteTo.Sink(sink)
				.CreateLogger();

			// Act: Write a test log event.
			logger.Information("Test message");

			// Assert: Verify that exactly one log event was captured.
			Assert.Single(logEvents);
			var logEvent = logEvents[0];

			// Verify that the event contains the enrichers’ properties.
			// WithThreadId adds a property named "ThreadId"
			Assert.True(logEvent.Properties.ContainsKey("ThreadId"), "Log event does not contain ThreadId property.");

			// WithMachineName adds a property named "MachineName"
			Assert.True(logEvent.Properties.ContainsKey("MachineName"), "Log event does not contain MachineName property.");

			// WithEnvironmentName typically adds a property named "Environment".
			Assert.True(logEvent.Properties.ContainsKey("EnvironmentName"), "Log event does not contain Environment property.");

			// FromLogContext doesn't add a property unless one is pushed, so no check is required.
		}
	}
}

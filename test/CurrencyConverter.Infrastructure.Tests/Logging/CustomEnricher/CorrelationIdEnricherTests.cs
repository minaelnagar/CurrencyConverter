using CurrencyConverter.Infrastructure.Logging.CustomEnrichers;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Logging.CustomEnricher
{
	public class CorrelationIdEnricherTests
	{
		private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
		private readonly CorrelationIdEnricher _enricher;

		public CorrelationIdEnricherTests()
		{
			_httpContextAccessor = new Mock<IHttpContextAccessor>();
			_enricher = new CorrelationIdEnricher(_httpContextAccessor.Object);
		}

		[Fact]
		public void Enrich_WithHttpContext_UsesTraceIdentifier()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			const string expectedCorrelationId = "test-correlation-id";
			httpContext.TraceIdentifier = expectedCorrelationId;
			_httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

			var logEvent = CreateLogEvent();
			var propertyFactory = new DummyLogEventPropertyFactory();

			// Act
			_enricher.Enrich(logEvent, propertyFactory);

			// Assert
			logEvent.Properties.Should().ContainKey("CorrelationId");
			var property = logEvent.Properties["CorrelationId"];
			property.ToString().Should().Contain(expectedCorrelationId);
		}

		[Fact]
		public void Enrich_WithoutHttpContext_UsesDefaultValue()
		{
			// Arrange
			_httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
			var logEvent = CreateLogEvent();
			var propertyFactory = new DummyLogEventPropertyFactory();

			// Act
			_enricher.Enrich(logEvent, propertyFactory);

			// Assert
			logEvent.Properties.Should().ContainKey("CorrelationId");
			var property = logEvent.Properties["CorrelationId"];
			property.ToString().Should().Contain("N/A");
		}

		private static LogEvent CreateLogEvent()
		{
			return new LogEvent(
				DateTimeOffset.UtcNow,
				LogEventLevel.Information,
				null,
				new MessageTemplate(new List<MessageTemplateToken>()),
				new List<LogEventProperty>());
		}
	}

}

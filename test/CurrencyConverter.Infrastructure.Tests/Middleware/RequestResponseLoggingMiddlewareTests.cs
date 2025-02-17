using CurrencyConverter.Infrastructure.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Middleware
{
	namespace CurrencyConverter.Infrastructure.Tests.Middleware
	{
		public class RequestResponseLoggingMiddlewareTests
		{
			[Fact]
			public async Task InvokeAsync_WithValidContext_LogsCorrectInformation()
			{
				// Arrange
				var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
				// Setup BeginScope to return a dummy disposable.
				var disposableMock = new Mock<IDisposable>();
				loggerMock.Setup(x => x.BeginScope(It.IsAny<object>())).Returns(disposableMock.Object);

				bool nextCalled = false;
				RequestDelegate next = ctx =>
				{
					nextCalled = true;
					// Set a specific response status code.
					ctx.Response.StatusCode = 200;
					return Task.CompletedTask;
				};

				var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object);

				var context = new DefaultHttpContext();
				context.TraceIdentifier = "test-correlation-id";
				context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
				context.Request.Method = "GET";
				context.Request.Path = "/test";
				// Create a user with a "client_id" claim.
				var identity = new ClaimsIdentity(new[] { new Claim("client_id", "client-123") });
				context.User = new ClaimsPrincipal(identity);
				// Assign a dummy response body.
				context.Response.Body = new MemoryStream();

				// Act
				await middleware.InvokeAsync(context);

				// Assert
				Assert.True(nextCalled, "Next delegate was not called.");

				// Verify that BeginScope was called with the expected dictionary.
				loggerMock.Verify(x => x.BeginScope(
					It.Is<Dictionary<string, object>>(dict =>
						dict.ContainsKey("CorrelationId") && dict["CorrelationId"].ToString() == "test-correlation-id" &&
						dict.ContainsKey("ClientIp") && dict["ClientIp"].ToString() == "127.0.0.1" &&
						dict.ContainsKey("ClientId") && dict["ClientId"].ToString() == "client-123"
					)), Times.Once);

				// Verify that LogInformation was called with a message containing the request details.
				loggerMock.Verify(x => x.Log(
					LogLevel.Information,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((state, t) =>
						state.ToString().Contains("Request GET /test completed in") &&
						state.ToString().Contains("with status 200")
					),
					null,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
					Times.Once);
			}

			[Fact]
			public async Task InvokeAsync_WithMissingClientInfo_UsesFallbackValues()
			{
				// Arrange
				var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
				var disposableMock = new Mock<IDisposable>();
				loggerMock.Setup(x => x.BeginScope(It.IsAny<object>())).Returns(disposableMock.Object);

				bool nextCalled = false;
				RequestDelegate next = ctx =>
				{
					nextCalled = true;
					ctx.Response.StatusCode = 200;
					return Task.CompletedTask;
				};

				var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object);

				var context = new DefaultHttpContext();
				context.TraceIdentifier = "fallback-correlation-id";
				// No remote IP is provided.
				context.Connection.RemoteIpAddress = null;
				context.Request.Method = "POST";
				context.Request.Path = "/fallback";
				// No "client_id" claim.
				context.User = new ClaimsPrincipal(new ClaimsIdentity());
				context.Response.Body = new MemoryStream();

				// Act
				await middleware.InvokeAsync(context);

				// Assert
				Assert.True(nextCalled, "Next delegate was not called.");

				// Verify that the fallback values ("unknown" for client IP, "anonymous" for client ID) are used.
				loggerMock.Verify(x => x.BeginScope(
					It.Is<Dictionary<string, object>>(dict =>
						dict.ContainsKey("CorrelationId") && dict["CorrelationId"].ToString() == "fallback-correlation-id" &&
						dict.ContainsKey("ClientIp") && dict["ClientIp"].ToString() == "unknown" &&
						dict.ContainsKey("ClientId") && dict["ClientId"].ToString() == "anonymous"
					)), Times.Once);

				loggerMock.Verify(x => x.Log(
					LogLevel.Information,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((state, t) =>
						state.ToString().Contains("Request POST /fallback completed in") &&
						state.ToString().Contains("with status 200")
					),
					null,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
					Times.Once);
			}
		}
	}
}

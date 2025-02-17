using CurrencyConverter.Infrastructure.Middleware;
using CurrencyConverter.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Middleware
{
	public class ApiThrottlingMiddlewareTests
	{
		// Helper: Creates a RateLimitSettings options instance.
		private IOptions<RateLimitSettings> CreateSettings(int permitLimit, int windowMinutes) =>
			Options.Create(new RateLimitSettings { PermitLimit = permitLimit, WindowMinutes = windowMinutes });

		[Fact]
		public async Task InvokeAsync_BelowLimit_IncrementsCount_AndCallsNext()
		{
			// Arrange
			int permitLimit = 5;
			int windowMinutes = 1;
			var settings = CreateSettings(permitLimit, windowMinutes);

			// Create a mock cache that returns null (i.e. count=0)
			var cacheMock = new Mock<IDistributedCache>();
			cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
					 .ReturnsAsync((byte[])null);
			cacheMock.Setup(c => c.SetAsync(
				It.IsAny<string>(),
				It.IsAny<byte[]>(),
				It.IsAny<DistributedCacheEntryOptions>(),
				default))
					 .Returns(Task.CompletedTask);

			bool nextCalled = false;
			RequestDelegate next = ctx =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			};

			var loggerMock = new Mock<ILogger<ApiThrottlingMiddleware>>();

			// Create an HttpContext with a user claim "client_id"
			var context = new DefaultHttpContext();
			var identity = new ClaimsIdentity();
			identity.AddClaim(new Claim("client_id", "test_client"));
			context.User = new ClaimsPrincipal(identity);

			var middleware = new ApiThrottlingMiddleware(next, cacheMock.Object, loggerMock.Object, settings);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.True(nextCalled, "Next delegate was not called when under limit.");
			string expectedKey = "throttle:test_client";
			// Verify GetAsync was called for our key.
			cacheMock.Verify(c => c.GetAsync(expectedKey, default), Times.AtLeastOnce);
			// Verify that SetAsync was called with a value representing count 1.
			cacheMock.Verify(c => c.SetAsync(
				expectedKey,
				It.Is<byte[]>(b => BitConverter.ToInt32(b, 0) == 1),
				It.Is<DistributedCacheEntryOptions>(opts =>
					Math.Abs(opts.AbsoluteExpirationRelativeToNow.Value.TotalMinutes - windowMinutes) < 0.01),
				default), Times.Once);
		}

		[Fact]
		public async Task InvokeAsync_AtLimit_Returns429AndWritesError()
		{
			// Arrange
			int permitLimit = 5;
			int windowMinutes = 1;
			var settings = CreateSettings(permitLimit, windowMinutes);

			var cacheMock = new Mock<IDistributedCache>();
			// Return a byte array representing count = permitLimit.
			byte[] countBytes = BitConverter.GetBytes(permitLimit);
			cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
					 .ReturnsAsync(countBytes);
			// Ensure SetAsync is not called.
			cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
				It.IsAny<DistributedCacheEntryOptions>(), default))
					 .Returns(Task.CompletedTask);

			bool nextCalled = false;
			RequestDelegate next = ctx =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			};

			var loggerMock = new Mock<ILogger<ApiThrottlingMiddleware>>();

			var context = new DefaultHttpContext();
			var identity = new ClaimsIdentity();
			identity.AddClaim(new Claim("client_id", "test_client"));
			context.User = new ClaimsPrincipal(identity);
			// Prepare an empty response body stream to capture output.
			context.Response.Body = new MemoryStream();

			var middleware = new ApiThrottlingMiddleware(next, cacheMock.Object, loggerMock.Object, settings);

			// Act
			await middleware.InvokeAsync(context);

			// Assert: Next delegate should not be called.
			Assert.False(nextCalled, "Next delegate should not be invoked when limit is reached.");
			Assert.Equal(429, context.Response.StatusCode);

			// Read the response body.
			context.Response.Body.Seek(0, SeekOrigin.Begin);
			using(var reader = new StreamReader(context.Response.Body, Encoding.UTF8))
			{
				string responseBody = await reader.ReadToEndAsync();
				Assert.Contains("Too many requests", responseBody);
			}

			// Verify that SetAsync was not called.
			cacheMock.Verify(c => c.SetAsync(
				It.IsAny<string>(),
				It.IsAny<byte[]>(),
				It.IsAny<DistributedCacheEntryOptions>(),
				default), Times.Never);
		}

		[Fact]
		public async Task InvokeAsync_UsesRemoteIp_WhenClientIdMissing()
		{
			// Arrange
			int permitLimit = 5;
			int windowMinutes = 1;
			var settings = CreateSettings(permitLimit, windowMinutes);

			var cacheMock = new Mock<IDistributedCache>();
			// Return null (i.e., count = 0)
			cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
					 .ReturnsAsync((byte[])null);
			cacheMock.Setup(c => c.SetAsync(
				It.IsAny<string>(),
				It.IsAny<byte[]>(),
				It.IsAny<DistributedCacheEntryOptions>(),
				default))
					 .Returns(Task.CompletedTask);

			bool nextCalled = false;
			RequestDelegate next = ctx =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			};

			var loggerMock = new Mock<ILogger<ApiThrottlingMiddleware>>();

			var context = new DefaultHttpContext();
			// Do not set user claims.
			context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

			var middleware = new ApiThrottlingMiddleware(next, cacheMock.Object, loggerMock.Object, settings);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.True(nextCalled, "Next delegate was not called when under limit.");
			string expectedKey = "throttle:192.168.1.100";
			cacheMock.Verify(c => c.GetAsync(expectedKey, default), Times.AtLeastOnce);
			cacheMock.Verify(c => c.SetAsync(
				expectedKey,
				It.Is<byte[]>(b => BitConverter.ToInt32(b, 0) == 1),
				It.IsAny<DistributedCacheEntryOptions>(),
				default), Times.Once);
		}

		[Fact]
		public async Task InvokeAsync_UsesAnonymous_WhenNoClientIdAndNoRemoteIp()
		{
			// Arrange
			int permitLimit = 5;
			int windowMinutes = 1;
			var settings = CreateSettings(permitLimit, windowMinutes);

			var cacheMock = new Mock<IDistributedCache>();
			cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
					 .ReturnsAsync((byte[])null);
			cacheMock.Setup(c => c.SetAsync(
				It.IsAny<string>(),
				It.IsAny<byte[]>(),
				It.IsAny<DistributedCacheEntryOptions>(),
				default))
					 .Returns(Task.CompletedTask);

			bool nextCalled = false;
			RequestDelegate next = ctx =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			};

			var loggerMock = new Mock<ILogger<ApiThrottlingMiddleware>>();

			var context = new DefaultHttpContext();
			// No user claim and no remote IP; fallback to "anonymous".
			var middleware = new ApiThrottlingMiddleware(next, cacheMock.Object, loggerMock.Object, settings);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.True(nextCalled, "Next delegate was not called when under limit.");
			string expectedKey = "throttle:anonymous";
			cacheMock.Verify(c => c.GetAsync(expectedKey, default), Times.AtLeastOnce);
			cacheMock.Verify(c => c.SetAsync(
				expectedKey,
				It.Is<byte[]>(b => BitConverter.ToInt32(b, 0) == 1),
				It.IsAny<DistributedCacheEntryOptions>(),
				default), Times.Once);
		}
	}
}

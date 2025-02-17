using CurrencyConverter.Application.Exceptions;
using CurrencyConverter.Infrastructure.Middleware;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Middleware
{
	public class ExceptionHandlingMiddlewareTests
	{
		// Helper method to read the entire response body as a string.
		private async Task<string> ReadResponseBodyAsync(HttpResponse response)
		{
			response.Body.Seek(0, SeekOrigin.Begin);
			using var reader = new StreamReader(response.Body, Encoding.UTF8);
			return await reader.ReadToEndAsync();
		}

		[Fact]
		public async Task InvokeAsync_NoException_PassesThrough()
		{
			// Arrange
			var context = new DefaultHttpContext();
			var originalBody = new MemoryStream();
			context.Response.Body = originalBody;
			context.Response.StatusCode = 200;
			// Next delegate does not throw.
			var nextCalled = false;
			RequestDelegate next = ctx =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			};

			var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
			var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.True(nextCalled);
			Assert.Equal(200, context.Response.StatusCode);
			// Logger.LogError should not be called.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.IsAny<It.IsAnyType>(),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Never);
		}

		[Fact]
		public async Task InvokeAsync_DomainException_Returns400AndErrorMessage()
		{
			// Arrange
			var context = new DefaultHttpContext();
			var responseBody = new MemoryStream();
			context.Response.Body = responseBody;
			// Next delegate throws a DomainException.
			var exception = new Domain.Exceptions.DomainException("Domain error occurred");
			RequestDelegate next = ctx => throw exception;

			var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
			var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

			// Act
			await middleware.InvokeAsync(context);

			// Assert: Use StartsWith to allow for charset in content type.
			Assert.StartsWith("application/json", context.Response.ContentType);
			Assert.Equal(400, context.Response.StatusCode);
			var body = await ReadResponseBodyAsync(context.Response);
			using var doc = JsonDocument.Parse(body);
			var error = doc.RootElement.GetProperty("error").GetString();
			Assert.Equal("Domain error occurred", error);

			// Verify logger was called with the exception.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unhandled exception occurred")),
					exception,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Fact]
		public async Task InvokeAsync_ValidationException_Returns400AndSerializedErrors()
		{
			// Arrange
			var failures = new List<ValidationFailure>
			{
				new ValidationFailure("Name", "Name is required"),
				new ValidationFailure("Age", "Age must be over 18")
			};
			var validationException = new ValidationException(failures);
			var context = new DefaultHttpContext();
			var responseBody = new MemoryStream();
			context.Response.Body = responseBody;
			// Next delegate throws the ValidationException.
			RequestDelegate next = ctx => throw validationException;

			var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
			var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.StartsWith("application/json", context.Response.ContentType);
			Assert.Equal(400, context.Response.StatusCode);
			var body = await ReadResponseBodyAsync(context.Response);
			using var doc = JsonDocument.Parse(body);
			var error = doc.RootElement.GetProperty("error").GetString();
			var expected = JsonSerializer.Serialize(validationException.Errors);
			Assert.Equal(expected, error);

			// Verify logger was called with the exception.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unhandled exception occurred")),
					validationException,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Fact]
		public async Task InvokeAsync_UnauthorizedAccessException_Returns401AndUnauthorizedMessage()
		{
			// Arrange
			var context = new DefaultHttpContext();
			var responseBody = new MemoryStream();
			context.Response.Body = responseBody;
			// Next delegate throws UnauthorizedAccessException.
			var exception = new UnauthorizedAccessException("Access denied");
			RequestDelegate next = ctx => throw exception;

			var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
			var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.StartsWith("application/json", context.Response.ContentType);
			Assert.Equal(401, context.Response.StatusCode);
			var body = await ReadResponseBodyAsync(context.Response);
			using var doc = JsonDocument.Parse(body);
			var error = doc.RootElement.GetProperty("error").GetString();
			Assert.Equal("Unauthorized", error);

			// Verify logger was called.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unhandled exception occurred")),
					exception,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Fact]
		public async Task InvokeAsync_GenericException_Returns500AndGenericMessage()
		{
			// Arrange
			var context = new DefaultHttpContext();
			var responseBody = new MemoryStream();
			context.Response.Body = responseBody;
			// Next delegate throws a generic exception.
			var exception = new Exception("Some error");
			RequestDelegate next = ctx => throw exception;

			var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
			var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

			// Act
			await middleware.InvokeAsync(context);

			// Assert
			Assert.StartsWith("application/json", context.Response.ContentType);
			Assert.Equal(500, context.Response.StatusCode);
			var body = await ReadResponseBodyAsync(context.Response);
			using var doc = JsonDocument.Parse(body);
			var error = doc.RootElement.GetProperty("error").GetString();
			Assert.Equal("An error occurred processing your request.", error);

			// Verify logger was called.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unhandled exception occurred")),
					exception,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}
	}
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Middleware
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch(Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			_logger.LogError(exception, "An unhandled exception occurred");

			var response = context.Response;
			response.ContentType = "application/json";

			var (statusCode, message) = exception switch
			{
				Domain.Exceptions.DomainException => (400, exception.Message),
				Application.Exceptions.ValidationException validationEx =>
					(400, JsonSerializer.Serialize(validationEx.Errors)),
				UnauthorizedAccessException => (401, "Unauthorized"),
				_ => (500, "An error occurred processing your request.")
			};

			response.StatusCode = statusCode;
			await response.WriteAsJsonAsync(new { error = message });
		}
	}
}

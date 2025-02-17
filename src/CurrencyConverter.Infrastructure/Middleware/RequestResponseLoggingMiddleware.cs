using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Middleware
{
	public class RequestResponseLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

		public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var sw = Stopwatch.StartNew();
			var correlationId = context.TraceIdentifier;
			var clientIp = context.Connection.RemoteIpAddress?.ToString();
			var clientId = context.User.FindFirst("client_id")?.Value;

			using(_logger.BeginScope(new Dictionary<string, object>
			{
				["CorrelationId"] = correlationId,
				["ClientIp"] = clientIp ?? "unknown",
				["ClientId"] = clientId ?? "anonymous"
			}))
			{
				try
				{
					await _next(context);
				}
				finally
				{
					sw.Stop();
					_logger.LogInformation(
						"Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
						context.Request.Method,
						context.Request.Path,
						sw.ElapsedMilliseconds,
						context.Response.StatusCode);
				}
			}
		}
	}
}

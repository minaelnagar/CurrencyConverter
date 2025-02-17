using CurrencyConverter.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Middleware
{
	public class ApiThrottlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IDistributedCache _cache;
		private readonly ILogger<ApiThrottlingMiddleware> _logger;
		private readonly int _permitLimit;
		private readonly TimeSpan _window;

		public ApiThrottlingMiddleware(
			RequestDelegate next,
			IDistributedCache cache,
			ILogger<ApiThrottlingMiddleware> logger,
			IOptions<RateLimitSettings> settings)
		{
			_next = next;
			_cache = cache;
			_logger = logger;
			_permitLimit = settings.Value.PermitLimit;
			_window = TimeSpan.FromMinutes(settings.Value.WindowMinutes);
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var clientId = context.User.FindFirst("client_id")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
			var key = $"throttle:{clientId}";

			var requestCount = await GetRequestCountAsync(key);
			if(requestCount >= _permitLimit)
			{
				context.Response.StatusCode = 429; // Too Many Requests
				await context.Response.WriteAsJsonAsync(new { error = "Too many requests" });
				return;
			}

			await IncrementRequestCountAsync(key);
			await _next(context);
		}

		private async Task<int> GetRequestCountAsync(string key)
		{
			var count = await _cache.GetAsync(key);
			return count == null ? 0 : BitConverter.ToInt32(count);
		}

		private async Task IncrementRequestCountAsync(string key)
		{
			var count = await GetRequestCountAsync(key);
			var bytes = BitConverter.GetBytes(count + 1);
			await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = _window
			});
		}
	}
}

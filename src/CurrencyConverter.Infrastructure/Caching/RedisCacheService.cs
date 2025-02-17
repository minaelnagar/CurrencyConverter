using CurrencyConverter.Application.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Caching
{
	public class RedisCacheService :ICacheService
	{
		private readonly IConnectionMultiplexer _redis;
		private readonly ILogger<RedisCacheService> _logger;

		public RedisCacheService(
			IConnectionMultiplexer redis,
			ILogger<RedisCacheService> logger)
		{
			_redis = redis;
			_logger = logger;
		}

		public virtual async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
		{
			try
			{
				var db = _redis.GetDatabase();
				var value = await db.StringGetAsync(key);

				if(!value.HasValue)
					return default;

				return JsonSerializer.Deserialize<T>(value!);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error retrieving value for key: {Key}", key);
				return default;
			}
		}

		public virtual async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
		{
			try
			{
				var db = _redis.GetDatabase();
				var serialized = JsonSerializer.Serialize(value);
				await db.StringSetAsync(key, serialized, expiration);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error setting value for key: {Key}", key);
			}
		}

		public virtual async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
		{
			try
			{
				var db = _redis.GetDatabase();
				await db.KeyDeleteAsync(key);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error removing key: {Key}", key);
			}
		}
	}
}

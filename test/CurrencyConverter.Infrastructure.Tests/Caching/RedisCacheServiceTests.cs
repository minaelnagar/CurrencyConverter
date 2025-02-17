using CurrencyConverter.Infrastructure.Caching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Caching
{
	public class RedisCacheServiceTests
	{
		// Helpers to create a RedisCacheService with mocked dependencies.
		private (RedisCacheService service, Mock<IConnectionMultiplexer> multiplexerMock, Mock<IDatabase> databaseMock, Mock<ILogger<RedisCacheService>> loggerMock)
			CreateService()
		{
			var multiplexerMock = new Mock<IConnectionMultiplexer>();
			var databaseMock = new Mock<IDatabase>();
			var loggerMock = new Mock<ILogger<RedisCacheService>>();

			// When GetDatabase is called, return our mocked database.
			multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
						   .Returns(databaseMock.Object);

			var service = new RedisCacheService(multiplexerMock.Object, loggerMock.Object);
			return (service, multiplexerMock, databaseMock, loggerMock);
		}

		#region GetAsync Tests

		[Fact]
		public async Task GetAsync_ReturnsDefault_WhenValueNotFound()
		{
			// Arrange
			var key = "missing_key";
			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			// Simulate cache miss: Redis returns a value with HasValue==false.
			databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
						.ReturnsAsync(RedisValue.Null);

			// Act
			var result = await service.GetAsync<string>(key);

			// Assert: result should be null (default for reference types)
			Assert.Null(result);
		}

		[Fact]
		public async Task GetAsync_ReturnsDeserializedValue_WhenValueExists()
		{
			// Arrange
			var key = "existing_key";
			var expected = "hello world";
			// Serialize the expected value.
			var serialized = JsonSerializer.Serialize(expected);

			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			// Simulate that the key exists.
			databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
						.ReturnsAsync((RedisValue)serialized);

			// Act
			var result = await service.GetAsync<string>(key);

			// Assert
			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task GetAsync_ReturnsDefault_AndLogsError_WhenExceptionThrown()
		{
			// Arrange
			var key = "exception_key";
			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			// Force an exception when calling StringGetAsync.
			var exception = new Exception("Test exception");
			databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
						.ThrowsAsync(exception);

			// Act
			var result = await service.GetAsync<string>(key);

			// Assert: should return default (null) when exception is thrown.
			Assert.Null(result);

			// Verify that logger.LogError was called with the exception and key.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving value for key:") && v.ToString().Contains(key)),
					exception,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		#endregion

		#region SetAsync Tests

		[Fact]
		public async Task SetAsync_CallsStringSetAsync_WithCorrectParameters()
		{
			// Arrange
			var key = "set_key";
			var value = new { Message = "test" };
			TimeSpan expiration = TimeSpan.FromMinutes(10);
			var expectedSerialized = JsonSerializer.Serialize(value);

			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			// Setup StringSetAsync to return true (simulate success).
			databaseMock.Setup(db => db.StringSetAsync(
					key,
					expectedSerialized,
					expiration,
					false,
					It.IsAny<When>(),
					It.IsAny<CommandFlags>()))
				.ReturnsAsync(true);

			// Act
			await service.SetAsync(key, value, expiration);

			// Assert: Verify that StringSetAsync was called with the serialized value, expiration, and the correct extra parameters.
			databaseMock.Verify(db => db.StringSetAsync(
					key,
					expectedSerialized,
					expiration,
					false,
					It.Is<When>(w => w == When.Always),
					It.Is<CommandFlags>(cf => cf == CommandFlags.None)
				), Times.Once);
		}

		[Fact]
		public async Task SetAsync_LogsError_WhenExceptionThrown()
		{
			// Arrange
			var key = "set_exception_key";
			var value = 123;
			TimeSpan expiration = TimeSpan.FromMinutes(5);
			var expectedSerialized = JsonSerializer.Serialize(value); // "123"

			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			var exception = new Exception("SetAsync exception");

			// Force StringSetAsync to throw an exception by matching the correct overload.
			databaseMock.Setup(db => db.StringSetAsync(
					key,
					expectedSerialized,
					expiration,
					false,
					When.Always,
					CommandFlags.None))
						.ThrowsAsync(exception);

			// Act
			await service.SetAsync(key, value, expiration);

			// Assert: Verify that logger.LogError was called with the exception and key.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) =>
						v.ToString().Contains("Error setting value for key:") &&
						v.ToString().Contains(key)),
					exception,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		#endregion

		#region RemoveAsync Tests

		[Fact]
		public async Task RemoveAsync_CallsKeyDeleteAsync_Successfully()
		{
			// Arrange
			var key = "remove_key";
			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			// Setup KeyDeleteAsync to return true (simulate successful deletion).
			databaseMock.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
						.ReturnsAsync(true);

			// Act
			await service.RemoveAsync(key);

			// Assert: Verify that KeyDeleteAsync was called.
			databaseMock.Verify(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
		}

		[Fact]
		public async Task RemoveAsync_LogsError_WhenExceptionThrown()
		{
			// Arrange
			var key = "remove_exception_key";
			var (service, multiplexerMock, databaseMock, loggerMock) = CreateService();

			var exception = new Exception("Remove exception");
			// Force KeyDeleteAsync to throw an exception.
			databaseMock.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
						.ThrowsAsync(exception);

			// Act
			await service.RemoveAsync(key);

			// Assert: Verify that logger.LogError was called with the exception and key.
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) =>
						v.ToString().Contains("Error removing key:") && v.ToString().Contains(key)),
					exception,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		#endregion
	}
}

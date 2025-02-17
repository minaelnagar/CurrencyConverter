using CurrencyConverter.Infrastructure.Authentication.Settings;
using CurrencyConverter.Infrastructure.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Authentication
{
	public class JwtTokenHandlerTests
	{
		private readonly JwtTokenHandler _tokenHandler;
		private readonly JwtSettings _settings;

		public JwtTokenHandlerTests()
		{
			_settings = new JwtSettings
			{
				Key = "your-super-secret-key-with-sufficient-length-for-testing",
				Issuer = "test",
				Audience = "test",
				ExpirationMinutes = 60
			};

			_tokenHandler = new JwtTokenHandler(Options.Create(_settings).Value);
		}

		[Fact]
		public void GenerateToken_WithValidInputs_ReturnsValidToken()
		{
			// Arrange
			var userId = "user123";
			var roles = new[] { "User", "Admin" };

			// Act
			var token = _tokenHandler.GenerateToken(userId, roles);

			// Assert
			token.Should().NotBeNullOrEmpty();
			_tokenHandler.ValidateToken(token).Should().BeTrue();
		}

		[Fact]
		public void ValidateToken_WithInvalidToken_ReturnsFalse()
		{
			// Arrange
			var invalidToken = "invalid.token.here";

			// Act
			var result = _tokenHandler.ValidateToken(invalidToken);

			// Assert
			result.Should().BeFalse();
		}
	}
}

using CurrencyConverter.API.Controllers.V1;
using CurrencyConverter.API.Models;
using CurrencyConverter.Infrastructure.Authentication;
using CurrencyConverter.Infrastructure.Authentication.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.API.Tests.Controllers
{
	public class DevControllerTests
	{
		private readonly Mock<IOptions<JwtSettings>> _settings;
		private readonly Mock<JwtTokenHandler> _tokenHandler;
		private readonly Mock<ILogger<DevController>> _logger;
		private readonly DevController _controller;
		private readonly DefaultHttpContext _httpContext;

		public DevControllerTests()
		{
			_settings = new Mock<IOptions<JwtSettings>>();
			_settings.Setup(x => x.Value).Returns(new JwtSettings
			{
				Key = "your-super-secret-key-with-sufficient-length-for-testing",
				Issuer = "test",
				Audience = "test",
				ExpirationMinutes = 60
			});

			_tokenHandler = new Mock<JwtTokenHandler>(_settings.Object.Value);
			_logger = new Mock<ILogger<DevController>>();
			_controller = new DevController(_tokenHandler.Object, _logger.Object);
			_httpContext = new DefaultHttpContext();
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = _httpContext
			};
		}

		[Fact]
		public void GenerateToken_WhenLocal_ReturnsToken()
		{
			// Arrange
			var request = new TokenRequest
			{
				UserId = "test",
				Roles = new List<string> { "User" }
			};

			_httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
			_tokenHandler.Setup(x => x.GenerateToken(
				It.IsAny<string>(),
				It.IsAny<IEnumerable<string>>()))
				.Returns("test-token");

			// Act
			var result = _controller.GenerateToken(request);

			// Assert
			var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
			okResult.Value.Should().BeEquivalentTo(new { token = "test-token" });
		}

		[Fact]
		public async Task GenerateToken_WhenNotLocal_ReturnsForbid()
		{
			// Arrange
			var request = new TokenRequest
			{
				UserId = "test",
				Roles = new List<string> { "User" }
			};

			_httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

			// Act
			var result = _controller.GenerateToken(request);

			// Assert
			result.Result.Should().BeOfType<ForbidResult>();
		}
	}
}

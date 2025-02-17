using CurrencyConverter.API.Extensions;
using CurrencyConverter.API.Models;
using CurrencyConverter.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CurrencyConverter.API.Controllers.V1
{
	/// <summary>
	/// Development-only endpoints for testing purposes
	/// </summary>
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class DevController :ControllerBase
	{
		private readonly JwtTokenHandler _tokenHandler;
		private readonly ILogger<DevController> _logger;

		public DevController(JwtTokenHandler tokenHandler, ILogger<DevController> logger)
		{
			_tokenHandler = tokenHandler;
			_logger = logger;
		}

		/// <summary>
		/// Generates a JWT token for testing (Development environment only)
		/// </summary>
		/// <param name="request">Token generation request</param>
		/// <remarks>
		/// Sample request:
		/// 
		///     POST /api/v1/dev/token
		///     {
		///         "userId": "test-user",
		///         "roles": ["User"]
		///     }
		/// 
		/// Sample response:
		/// 
		///     {
		///         "token": "eyJhbGciOiJIUzI1NiIs..."
		///     }
		/// 
		/// Available roles:
		/// * User - Basic access to API endpoints
		/// * Admin - Advanced access (if implemented)
		/// 
		/// Note: This endpoint is only available in the Development environment
		/// </remarks>
		/// <returns>JWT token for testing</returns>
		/// <response code="200">Returns the generated token</response>
		/// <response code="403">If accessed in non-development environment</response>
		[HttpPost("token")]
		public ActionResult<string> GenerateToken([FromBody] TokenRequest request)
		{
			if(!HttpContext.Request.IsLocal())
			{
				return Forbid();
			}

			var token = _tokenHandler.GenerateToken(request.UserId, request.Roles);
			return Ok(new { token });
		}
	}
}

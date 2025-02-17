using CurrencyConverter.Infrastructure.Authentication.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Authentication
{
	public class JwtTokenHandler
	{
		private readonly JwtSettings _settings;

		public JwtTokenHandler(JwtSettings settings)
		{
			_settings = settings;
		}

		public virtual string GenerateToken(string userId, IEnumerable<string> roles)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, userId),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			};

			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var token = new JwtSecurityToken(
				issuer: _settings.Issuer,
				audience: _settings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public virtual bool ValidateToken(string token)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes(_settings.Key);

			try
			{
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = true,
					ValidIssuer = _settings.Issuer,
					ValidateAudience = true,
					ValidAudience = _settings.Audience,
					ClockSkew = TimeSpan.Zero
				}, out _);

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

using System.Diagnostics.CodeAnalysis;

namespace CurrencyConverter.API.Models
{
	[ExcludeFromCodeCoverage]
	public class TokenRequest
	{
		public string UserId { get; set; } = null!;
		public List<string> Roles { get; set; } = new();
	}
}

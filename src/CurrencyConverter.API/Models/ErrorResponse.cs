using System.Diagnostics.CodeAnalysis;

namespace CurrencyConverter.API.Models
{
	/// <summary>
	/// Represents an error response
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ErrorResponse
	{
		/// <summary>
		/// Error message or messages
		/// </summary>
		public string[] Messages { get; set; } = Array.Empty<string>();

		/// <summary>
		/// Optional error code
		/// </summary>
		public string? Code { get; set; }
	}

}

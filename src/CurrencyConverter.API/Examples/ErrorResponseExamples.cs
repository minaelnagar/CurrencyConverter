using CurrencyConverter.API.Models;
using CurrencyConverter.Application.Models.Responses;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace CurrencyConverter.API.Examples
{
	[ExcludeFromCodeCoverage]
	public class ValidationErrorExample :IExamplesProvider<ErrorResponse>
	{
		public ErrorResponse GetExamples()
		{
			return new ErrorResponse
			{
				Code = "ValidationError",
				Messages = new[]
				{
				"Base currency is required",
				"Start date must be before end date"
			}
			};
		}
	}
	[ExcludeFromCodeCoverage]
	public class RateLimitErrorExample :IExamplesProvider<ErrorResponse>
	{
		public ErrorResponse GetExamples()
		{
			return new ErrorResponse
			{
				Code = "RateLimitExceeded",
				Messages = new[] { "Rate limit exceeded. Try again in 60 seconds." }
			};
		}
	}
}

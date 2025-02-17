using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Models.Responses
{
	public record CurrencyConversionResponse
	{
		public required string FromCurrency { get; init; }
		public required string ToCurrency { get; init; }
		public required decimal Amount { get; init; }
		public required decimal ConvertedAmount { get; init; }
		public required decimal Rate { get; init; }
	}
}

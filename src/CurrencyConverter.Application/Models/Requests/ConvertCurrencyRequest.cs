using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Models.Requests
{
	public record ConvertCurrencyRequest
	{
		public required string FromCurrency { get; init; }
		public required string ToCurrency { get; init; }
		public required decimal Amount { get; init; }
	}
}

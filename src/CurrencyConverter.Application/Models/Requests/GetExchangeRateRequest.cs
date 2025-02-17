using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Models.Requests
{
	public record GetExchangeRateRequest
	{
		public string? BaseCurrency { get; init; }

	}
}

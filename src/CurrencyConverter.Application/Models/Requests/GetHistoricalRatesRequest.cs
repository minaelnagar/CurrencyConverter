using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Models.Requests
{
	public record GetHistoricalRatesRequest
	{
		public required string BaseCurrency { get; init; }
		public required DateTime StartDate { get; init; }
		public required DateTime EndDate { get; init; }
		public int Page { get; init; } = 1;
		public int PageSize { get; init; } = 10;
	}
}

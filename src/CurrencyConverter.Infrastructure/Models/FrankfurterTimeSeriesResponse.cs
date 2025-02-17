using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Models
{
	public class FrankfurterTimeSeriesResponse
	{
		[JsonPropertyName("base")]
		public string Base { get; set; } = null!;

		[JsonPropertyName("start_date")]
		public DateTime StartDate { get; set; }

		[JsonPropertyName("end_date")]
		public DateTime EndDate { get; set; }

		[JsonPropertyName("rates")]
		public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; set; } = new();
	}
}

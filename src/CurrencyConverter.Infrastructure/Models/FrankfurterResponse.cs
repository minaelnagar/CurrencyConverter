﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Models
{
	public class FrankfurterResponse
	{
		[JsonPropertyName("base")]
		public string Base { get; set; } = null!;

		[JsonPropertyName("date")]
		public DateTime Date { get; set; }

		[JsonPropertyName("rates")]
		public Dictionary<string, decimal> Rates { get; set; } = new();
	}
}

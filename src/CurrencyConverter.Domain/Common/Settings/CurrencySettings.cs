using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Common.Settings
{
	public class CurrencySettings
	{
		public string DefaultBaseCurrency { get; set; } = "EUR";
		public List<string> RestrictedCurrencies { get; set; } = new();

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Settings
{
	public class RateLimitSettings
	{
		public int PermitLimit { get; set; } = 100;
		public int WindowMinutes { get; set; } = 1;
	}
}

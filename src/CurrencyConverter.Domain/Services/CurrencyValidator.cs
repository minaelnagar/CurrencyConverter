using CurrencyConverter.Domain.Common.Extensions;
using CurrencyConverter.Domain.Common.Settings;
using CurrencyConverter.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CurrencyConverter.Domain.Services
{
	public class CurrencyValidator
	{
		private readonly CurrencySettings _settings;

		public CurrencyValidator(CurrencySettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public virtual bool IsRestricted(string currencyCode)
		{
			string validCurrency = currencyCode.ValidateCurrencyCode();

			return _settings.RestrictedCurrencies.Contains(
				validCurrency,
				StringComparer.OrdinalIgnoreCase);
		}

		public virtual void CheckIsRestricted(string currencyCode)
		{
			if(IsRestricted(currencyCode))
			{
				throw new DomainException($"Currency {currencyCode} is restricted");
			}
		}

		public virtual  string GetDefaultBaseCurrency()
		{
			return _settings.DefaultBaseCurrency.ValidateCurrencyCode();
		}
	}
}

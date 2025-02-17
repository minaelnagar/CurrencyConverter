using CurrencyConverter.Application.Models.Requests;
using CurrencyConverter.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Interfaces
{
	public interface ICurrencyConversionService
	{
		Task<CurrencyConversionResponse> ConvertAsync(
			ConvertCurrencyRequest request,
			CancellationToken cancellationToken = default);
	}
}

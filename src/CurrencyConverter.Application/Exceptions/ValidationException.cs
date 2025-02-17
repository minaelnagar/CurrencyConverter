using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Exceptions
{
	public class ValidationException :Exception
	{
		public IReadOnlyDictionary<string, string[]> Errors { get; }

		public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
		{
			Errors = failures
				.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
				.ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
		}
	}
}

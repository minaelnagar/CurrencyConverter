using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Models.Responses
{
	public record PagedResponse<T>
	{
		public required IReadOnlyList<T> Items { get; init; }
		public required int CurrentPage { get; init; }
		public required int PageSize { get; init; }
		public required int TotalPages { get; init; }
		public required int TotalItems { get; init; }
		public bool HasPrevious => CurrentPage > 1;
		public bool HasNext => CurrentPage < TotalPages;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Tests.Models.Responses
{
	public class PagedResponseTests
	{
		[Fact]
		public void Constructor_SetsPropertiesCorrectly_And_ComputedPropertiesAreCorrect()
		{
			// Arrange
			var items = new List<int> { 1, 2, 3 };
			int currentPage = 1;
			int pageSize = 10;
			int totalPages = 3;
			int totalItems = 25;

			// Act
			var response = new PagedResponse<int>
			{
				Items = items,
				CurrentPage = currentPage,
				PageSize = pageSize,
				TotalPages = totalPages,
				TotalItems = totalItems
			};

			// Assert
			Assert.Equal(items, response.Items);
			Assert.Equal(currentPage, response.CurrentPage);
			Assert.Equal(pageSize, response.PageSize);
			Assert.Equal(totalPages, response.TotalPages);
			Assert.Equal(totalItems, response.TotalItems);

			// When on the first page and more pages exist:
			Assert.False(response.HasPrevious);
			Assert.True(response.HasNext);
		}

		[Fact]
		public void ComputedProperties_MiddlePage_ReturnsTrueForBoth()
		{
			// Arrange & Act
			var response = new PagedResponse<string>
			{
				Items = new List<string> { "a", "b", "c" },
				CurrentPage = 2,
				PageSize = 3,
				TotalPages = 3,
				TotalItems = 9
			};

			// Assert
			Assert.True(response.HasPrevious);  // 2 > 1
			Assert.True(response.HasNext);      // 2 < 3
		}

		[Fact]
		public void ComputedProperties_FirstPageWithSinglePage_ReturnsFalseForBoth()
		{
			// Arrange & Act
			var response = new PagedResponse<string>
			{
				Items = new List<string> { "a", "b", "c" },
				CurrentPage = 1,
				PageSize = 3,
				TotalPages = 1,
				TotalItems = 3
			};

			// Assert
			Assert.False(response.HasPrevious); // On first page
			Assert.False(response.HasNext);     // TotalPages == 1
		}

		[Fact]
		public void ComputedProperties_LastPage_ReturnsTrueForPreviousFalseForNext()
		{
			// Arrange & Act
			var response = new PagedResponse<string>
			{
				Items = new List<string> { "a", "b", "c" },
				CurrentPage = 3,
				PageSize = 3,
				TotalPages = 3,
				TotalItems = 9
			};

			// Assert
			Assert.True(response.HasPrevious);  // 3 > 1
			Assert.False(response.HasNext);     // 3 == TotalPages
		}

		[Fact]
		public void RecordEquality_TwoIdenticalResponses_AreEqual()
		{
			// Arrange
			var items = new List<double> { 1.1, 2.2, 3.3 };
			var response1 = new PagedResponse<double>
			{
				Items = items,
				CurrentPage = 2,
				PageSize = 5,
				TotalPages = 4,
				TotalItems = 20
			};

			var response2 = new PagedResponse<double>
			{
				Items = items,
				CurrentPage = 2,
				PageSize = 5,
				TotalPages = 4,
				TotalItems = 20
			};

			// Act & Assert
			Assert.Equal(response1, response2);
			Assert.True(response1 == response2);
		}

		[Fact]
		public void WithExpression_ModifiesProperty_AndComputedPropertiesUpdate()
		{
			// Arrange
			var response = new PagedResponse<int>
			{
				Items = new List<int> { 1, 2, 3 },
				CurrentPage = 1,
				PageSize = 10,
				TotalPages = 3,
				TotalItems = 25
			};

			// Act: Use with expression to change CurrentPage to 2.
			var modified = response with { CurrentPage = 2 };

			// Assert
			Assert.Equal(2, modified.CurrentPage);
			// On page 2 of 3, there should be both previous and next pages.
			Assert.True(modified.HasPrevious);
			Assert.True(modified.HasNext);
		}
	}
}

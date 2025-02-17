using CurrencyConverter.Infrastructure.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Infrastructure.Tests.Swagger;

public class SwaggerVersioningFilterTests
{
	// Helper to create an OperationFilterContext.
	private OperationFilterContext CreateContext(IEnumerable<object> endpointMetadata)
	{
		// Create a dummy ActionDescriptor with the provided endpoint metadata.
		var actionDescriptor = new ActionDescriptor
		{
			EndpointMetadata = endpointMetadata.ToList()
		};

		// Create a dummy ApiDescription.
		var apiDescription = new ApiDescription
		{
			ActionDescriptor = actionDescriptor
		};

		// Create a dummy ISchemaGenerator.
		var schemaGeneratorMock = new Mock<ISchemaGenerator>();
		var schemaGenerator = schemaGeneratorMock.Object;

		// Create a SchemaRepository.
		var schemaRepository = new SchemaRepository();

		// Get a dummy MethodInfo (using a method from object).
		var controllerType = typeof(object);
		var methodInfo = controllerType.GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public);

		// Construct and return the OperationFilterContext.
		return new OperationFilterContext(apiDescription, schemaGenerator, schemaRepository, methodInfo);
	}

	[Fact]
	public void Apply_NoApiVersionAttribute_DoesNotChangeOperation()
	{
		// Arrange
		var operation = new OpenApiOperation
		{
			Summary = "Original summary",
			Description = "Original description"
		};

		var context = CreateContext(new List<object>());
		var filter = new SwaggerVersioningFilter();

		// Act
		filter.Apply(operation, context);

		// Assert: Operation remains unchanged.
		Assert.Equal("Original summary", operation.Summary);
		Assert.Equal("Original description", operation.Description);
	}

	[Fact]
	public void Apply_WithNonDeprecatedApiVersionAttribute_PrefixesSummaryOnly()
	{
		// Arrange
		var apiVersionAttr = new ApiVersionAttribute("1.0")
		{
			Deprecated = false
		};

		var operation = new OpenApiOperation
		{
			Summary = "Original summary",
			Description = "Original description"
		};

		var context = CreateContext(new List<object> { apiVersionAttr });
		var filter = new SwaggerVersioningFilter();

		// Act
		filter.Apply(operation, context);

		// Assert: Summary is prefixed with version; description remains unchanged.
		Assert.Equal("[v1.0] Original summary", operation.Summary);
		Assert.Equal("Original description", operation.Description);
	}

	[Fact]
	public void Apply_WithDeprecatedApiVersionAttribute_PrefixesSummaryAndDescription()
	{
		// Arrange
		var apiVersionAttr = new ApiVersionAttribute("2.0")
		{
			Deprecated = true
		};

		var operation = new OpenApiOperation
		{
			Summary = "Original summary",
			Description = "Original description"
		};

		var context = CreateContext(new List<object> { apiVersionAttr });
		var filter = new SwaggerVersioningFilter();

		// Act
		filter.Apply(operation, context);

		// Assert: Both summary and description are prefixed.
		Assert.Equal("[v2.0] Original summary", operation.Summary);
		Assert.Equal("Warning: This API version has been deprecated.\nOriginal description", operation.Description);
	}

	[Fact]
	public void Apply_WithMultipleApiVersionAttributes_UsesFirstForSummaryAndDeprecatedForDescription()
	{
		// Arrange: First attribute is non-deprecated; second is deprecated.
		var attr1 = new ApiVersionAttribute("1.0")
		{
			Deprecated = false
		};
		var attr2 = new ApiVersionAttribute("2.0")
		{
			Deprecated = true
		};

		var operation = new OpenApiOperation
		{
			Summary = "Original summary",
			Description = "Original description"
		};

		var context = CreateContext(new List<object> { attr1, attr2 });
		var filter = new SwaggerVersioningFilter();

		// Act
		filter.Apply(operation, context);

		// Assert:
		// The first attribute (attr1) is used for the summary.
		Assert.Equal("[v1.0] Original summary", operation.Summary);
		// Since one attribute is deprecated (attr2), the description gets the warning.
		Assert.Equal("Warning: This API version has been deprecated.\nOriginal description", operation.Description);
	}

	[Fact]
	public void Apply_WithApiVersionAttributeWithEmptyVersion_ThrowFormatException()
	{
		// Arrange: Create an ApiVersionAttribute with an empty version.
		var apiVersionAttr = new ApiVersionAttribute(string.Empty)
		{
			Deprecated = false
		};

		var operation = new OpenApiOperation
		{
			Summary = "Original summary",
			Description = "Original description"
		};

		var context = CreateContext(new List<object> { apiVersionAttr });
		var filter = new SwaggerVersioningFilter();

		// Act
		var act = ()=> filter.Apply(operation, context);

		// Assert:
		act.Should().Throw<FormatException>();
	}
}

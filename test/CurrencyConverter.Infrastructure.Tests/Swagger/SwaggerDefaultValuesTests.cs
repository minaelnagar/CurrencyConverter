using CurrencyConverter.Infrastructure.Swagger;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Tests.Swagger
{
	public class SwaggerDefaultValuesTests
	{
		[Fact]
		public void Apply_NoObsoleteAttribute_NoContentRemoval()
		{
			// Arrange: No obsolete attribute.
			var customAttributes = new List<object>(); // No ObsoleteAttribute.

			// Supported response: non-default response (status code 200)
			var apiResponseFormat = new ApiResponseFormat { MediaType = "application/json" };
			var responseType = new ApiResponseType
			{
				StatusCode = 200,
				IsDefaultResponse = false,
				ApiResponseFormats = new List<ApiResponseFormat> { apiResponseFormat }
			};
			var supportedResponseTypes = new List<ApiResponseType> { responseType };

			var context = OperationFilterContextHelper.CreateContext(customAttributes, supportedResponseTypes);

			// Build an operation with a response having one content key that matches.
			var operation = new OpenApiOperation
			{
				Summary = "Test summary",
				Description = "Test description",
				Responses = new OpenApiResponses()
			};
			operation.Responses["200"] = new OpenApiResponse
			{
				Content = new Dictionary<string, OpenApiMediaType>
			{
				{ "application/json", new OpenApiMediaType() }
			}
			};

			var filter = new SwaggerDefaultValues();

			// Act
			filter.Apply(operation, context);

			// Assert
			// No obsolete attribute so Deprecated remains false.
			Assert.False(operation.Deprecated);
			// The content remains unchanged.
			Assert.Single(operation.Responses["200"].Content);
			Assert.True(operation.Responses["200"].Content.ContainsKey("application/json"));
		}

		[Fact]
		public void Apply_RemovesNonMatchingContentKeys_ForNonDefaultResponse()
		{
			// Arrange: No obsolete attribute.
			var customAttributes = new List<object>();
			// Supported response type: non-default, status code 200, only supports "application/json".
			var apiResponseFormat = new ApiResponseFormat { MediaType = "application/json" };
			var responseType = new ApiResponseType
			{
				StatusCode = 200,
				IsDefaultResponse = false,
				ApiResponseFormats = new List<ApiResponseFormat> { apiResponseFormat }
			};
			var supportedResponseTypes = new List<ApiResponseType> { responseType };

			var context = OperationFilterContextHelper.CreateContext(customAttributes, supportedResponseTypes);

			// Create an operation with a response that has two content types.
			var operation = new OpenApiOperation
			{
				Summary = "Test summary",
				Description = "Test description",
				Responses = new OpenApiResponses()
			};
			operation.Responses["200"] = new OpenApiResponse
			{
				Content = new Dictionary<string, OpenApiMediaType>
			{
				{ "application/json", new OpenApiMediaType() },
				{ "text/xml", new OpenApiMediaType() } // Not supported by ApiResponseFormats.
            }
			};

			var filter = new SwaggerDefaultValues();

			// Act
			filter.Apply(operation, context);

			// Assert: "text/xml" should be removed.
			var content = operation.Responses["200"].Content;
			Assert.Single(content);
			Assert.True(content.ContainsKey("application/json"));
			Assert.False(content.ContainsKey("text/xml"));
		}

		[Fact]
		public void Apply_DefaultResponse_UsesDefaultKeyAndRemovesNonMatchingKeys()
		{
			// Arrange: No obsolete attribute.
			var customAttributes = new List<object>();
			// Supported response type: default response.
			var apiResponseFormat = new ApiResponseFormat { MediaType = "application/json" };
			var responseType = new ApiResponseType
			{
				StatusCode = 200, // Ignored because IsDefaultResponse = true.
				IsDefaultResponse = true,
				ApiResponseFormats = new List<ApiResponseFormat> { apiResponseFormat }
			};
			var supportedResponseTypes = new List<ApiResponseType> { responseType };

			var context = OperationFilterContextHelper.CreateContext(customAttributes, supportedResponseTypes);

			// Create an operation with a "default" response having two content types.
			var operation = new OpenApiOperation
			{
				Summary = "Test summary",
				Description = "Test description",
				Responses = new OpenApiResponses()
			};
			operation.Responses["default"] = new OpenApiResponse
			{
				Content = new Dictionary<string, OpenApiMediaType>
			{
				{ "application/json", new OpenApiMediaType() },
				{ "application/xml", new OpenApiMediaType() } // Not supported.
            }
			};

			var filter = new SwaggerDefaultValues();

			// Act
			filter.Apply(operation, context);

			// Assert: "application/xml" should be removed.
			var content = operation.Responses["default"].Content;
			Assert.Single(content);
			Assert.True(content.ContainsKey("application/json"));
			Assert.False(content.ContainsKey("application/xml"));
		}
	}


	// A helper to create a dummy OperationFilterContext.
	public static class OperationFilterContextHelper
	{
		public static OperationFilterContext CreateContext(IEnumerable<object> customAttributes, IList<ApiResponseType> supportedResponseTypes)
		{
			var actionDescriptor = new ActionDescriptor
			{
				EndpointMetadata = customAttributes.ToList(),
			};

			ApiDescription apiDescription = new ApiDescription()
			{
				ActionDescriptor = actionDescriptor,
			};

			foreach(var responseType in supportedResponseTypes)
			{
				apiDescription.SupportedResponseTypes.Add(responseType);
			}

			// Create a dummy ISchemaGenerator.
			var schemaGeneratorMock = new Moq.Mock<ISchemaGenerator>();
			var schemaGenerator = schemaGeneratorMock.Object;

			var schemaRepository = new SchemaRepository();

			// Use any method (here, using Object.ToString)
			var controllerType = typeof(object);
			var methodInfo = controllerType.GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public);

			return new OperationFilterContext(apiDescription, schemaGenerator, schemaRepository, methodInfo);
		}
	}
}

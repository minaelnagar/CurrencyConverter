using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Swagger
{
	public class SwaggerVersioningFilter :IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
			var apiVersion = actionMetadata
				.OfType<ApiVersionAttribute>()
				.FirstOrDefault();
			var deprecatedApiVersion = actionMetadata
				.OfType<ApiVersionAttribute>()
				.FirstOrDefault(x => x.Deprecated);

			if(apiVersion != null)
			{
				operation.Summary = $"[v{apiVersion.Versions.First().ToString()}] {operation.Summary}";
			}

			if(deprecatedApiVersion != null)
			{
				operation.Description = "Warning: This API version has been deprecated.\n" + operation.Description;
			}
		}
	}
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Infrastructure.Swagger
{
	public class SwaggerDefaultValues :IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var apiDescription = context.ApiDescription;

			var deprecatedAttribute = apiDescription.CustomAttributes()
				.OfType<ObsoleteAttribute>()
				.Any();

			operation.Deprecated = deprecatedAttribute;

			foreach(var responseType in context.ApiDescription.SupportedResponseTypes)
			{
				var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
				var response = operation.Responses[responseKey];

				foreach(var contentType in response.Content.Keys)
				{
					if(!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
					{
						response.Content.Remove(contentType);
					}
				}
			}
		}
	}
}

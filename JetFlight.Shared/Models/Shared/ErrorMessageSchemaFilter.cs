using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Shared
{
    public class ErrorMessageSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Skip if schema has no required properties.
            if (!schema.Required.Any())
            {
                return;
            }

            var propertyWithAttribute = context.Type
                .GetProperties()
                .Select(p => (p.Name, p.GetCustomAttribute<RegularExpressionAttribute>()))
                .Where(tuple => tuple.Item2 != null)
                .ToList();

            foreach (var (name, required) in propertyWithAttribute)
            {
                // Will throw for property name of length 1...
                var pascalCaseName = char.ToLowerInvariant(name[0]) + name[1..];

                if (schema.Properties.TryGetValue(pascalCaseName, out var property))
                {

                    property.Properties.Add("RequiredErrorMessage", new OpenApiSchema
                    {
                        Title = required.ErrorMessage
                    });
                }
            }
        }
    }
}

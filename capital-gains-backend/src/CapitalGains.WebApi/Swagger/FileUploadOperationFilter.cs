using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CapitalGains.WebApi.Swagger;

/// <summary>
/// Operation filter to handle file upload documentation in Swagger
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToArray();

        if (!fileParameters.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "Arquivo contendo operações (.txt ou .json)"
                            }
                        },
                        Required = new HashSet<string> { "file" }
                    }
                }
            }
        };

        // Remove file parameter from parameters list since it's now in request body
        operation.Parameters = operation.Parameters?
            .Where(p => !fileParameters.Any(fp => fp.Name == p.Name))
            .ToList();
    }
}
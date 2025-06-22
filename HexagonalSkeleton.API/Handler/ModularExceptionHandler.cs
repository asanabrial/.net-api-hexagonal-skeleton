using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.API.Handler.ExceptionMapping;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexagonalSkeleton.API.Handler
{    /// <summary>
    /// Modern exception handler using the strategy pattern for better maintainability.
    /// Delegates exception mapping to specialized mappers for each layer.
    /// Follows SOLID principles and supports easy extension.
    /// </summary>
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly ExceptionMappingService _mappingService;
        
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandler(ILogger<ExceptionHandler> logger, ExceptionMappingService mappingService)
        {
            _logger = logger;
            _mappingService = mappingService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            // Special handling for ValidationException to maintain the exact same structure
            if (exception is ValidationException validationEx)
            {
                _logger.LogWarning("Handling validation exception with {ErrorCount} errors", validationEx.Errors.Count);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";

                var response = new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "Validation failed",
                    status = StatusCodes.Status400BadRequest,
                    instance = context.Request.Path.Value,
                    errors = validationEx.Errors
                };

                var responseJson = JsonSerializer.Serialize(response, SerializerOptions);
                await context.Response.WriteAsync(responseJson, cancellationToken);
                return true;
            }            // Try to map the exception using the mapping service
            var problemDetails = _mappingService.TryMapToProblemDetails(exception, context.Request.Path.Value ?? "");
            
            if (problemDetails == null)
                return false; // Let ASP.NET Core handle it

            _logger.LogWarning("ExceptionHandler: Handling custom exception: {ExceptionType} - {Message}", 
                exception.GetType().Name, exception.Message);

            context.Response.StatusCode = problemDetails.Status ?? 500;
            context.Response.ContentType = "application/problem+json";
            
            var json = JsonSerializer.Serialize(problemDetails, SerializerOptions);
            await context.Response.WriteAsync(json, cancellationToken);
            
            return true;
        }
    }
}

using HexagonalSkeleton.CommonCore.Constants;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexagonalSkeleton.API.Handler
{
    public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
    {
        

        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(
                exception, "Exception occurred: {Message}", exception.Message);

            var problemDetails = CreateProblemDetails(context, exception);
            var json = ToJson(problemDetails);

            const string contentType = "application/problem+json";
            context.Response.ContentType = contentType;
            await context.Response.WriteAsync(json, cancellationToken);

            return true;
        }

        private ProblemDetails CreateProblemDetails(in HttpContext context, in Exception exception)
        {
            var statusCode = context.Response.StatusCode;
            var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
            reasonPhrase = string.IsNullOrWhiteSpace(reasonPhrase) ? AppErrorMessage.UnhandledExceptionMsg : reasonPhrase;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = reasonPhrase,
            };

            if (env.IsProduction()) return problemDetails;

            problemDetails.Detail = exception.ToString();
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["data"] = exception.Data;

            return problemDetails;
        }

        private string ToJson(in ProblemDetails problemDetails)
        {
            try
            {
                return JsonSerializer.Serialize(problemDetails, SerializerOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, AppErrorMessage.ToJsonExceptionMsg);
            }

            return string.Empty;
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Service that coordinates exception mapping using the chain of responsibility pattern
    /// </summary>
    public class ExceptionMappingService
    {
        private readonly IEnumerable<IExceptionMapper> _mappers;
        private readonly ILogger<ExceptionMappingService> _logger;

        public ExceptionMappingService(IEnumerable<IExceptionMapper> mappers, ILogger<ExceptionMappingService> logger)
        {
            _mappers = mappers;
            _logger = logger;
        }

        /// <summary>
        /// Attempts to map an exception to ProblemDetails using the available mappers
        /// </summary>
        public ProblemDetails? TryMapToProblemDetails(Exception exception, string requestPath)
        {
            var mapper = _mappers.FirstOrDefault(m => m.CanHandle(exception));
            
            if (mapper == null)
            {
                _logger.LogDebug("No mapper found for exception type {ExceptionType}", exception.GetType().Name);
                return null;
            }

            try
            {
                var problemDetails = mapper.MapToProblemDetails(exception, requestPath);
                _logger.LogDebug("Successfully mapped {ExceptionType} using {MapperType}", 
                    exception.GetType().Name, mapper.GetType().Name);
                return problemDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping exception {ExceptionType} using {MapperType}", 
                    exception.GetType().Name, mapper.GetType().Name);
                return null;
            }
        }

        /// <summary>
        /// Checks if any mapper can handle the given exception
        /// </summary>
        public bool CanHandle(Exception exception)
        {
            return _mappers.Any(m => m.CanHandle(exception));
        }
    }
}

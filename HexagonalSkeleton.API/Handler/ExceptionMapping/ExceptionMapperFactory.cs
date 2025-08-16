using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    public class ExceptionMapperFactory : IExceptionMapperFactory
    {
        private readonly Dictionary<Type, IExceptionMapper> _mappers = new();
        private readonly IServiceProvider _serviceProvider;

        public ExceptionMapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeDefaultMappers();
        }

        private void InitializeDefaultMappers()
        {
            // Register default mappers - this can be extended without modifying existing code
            RegisterMapper<HexagonalSkeleton.Domain.Exceptions.DomainException>(
                _serviceProvider.GetRequiredService<DomainExceptionMapper>());
            RegisterMapper<HexagonalSkeleton.Application.Exceptions.ApplicationException>(
                _serviceProvider.GetRequiredService<ApplicationExceptionMapper>());
            RegisterMapper<HexagonalSkeleton.Application.Exceptions.ValidationException>(
                _serviceProvider.GetRequiredService<ValidationExceptionMapper>());
        }

        public IExceptionMapper CreateMapper(Type exceptionType)
        {
            // Try exact match first
            if (_mappers.TryGetValue(exceptionType, out var mapper))
            {
                return mapper;
            }

            // Try inheritance hierarchy
            var currentType = exceptionType.BaseType;
            while (currentType != null)
            {
                if (_mappers.TryGetValue(currentType, out mapper))
                {
                    return mapper;
                }
                currentType = currentType.BaseType;
            }

            // Fallback to a default mapper
            return new DefaultExceptionMapper();
        }

        public void RegisterMapper<TException>(IExceptionMapper mapper) where TException : Exception
        {
            _mappers[typeof(TException)] = mapper;
        }
    }
}

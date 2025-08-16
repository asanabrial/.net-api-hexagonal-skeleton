namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Factory for creating exception mappers
    /// Follows Open/Closed Principle - open for extension, closed for modification
    /// </summary>
    public interface IExceptionMapperFactory
    {
        IExceptionMapper CreateMapper(Type exceptionType);
        void RegisterMapper<TException>(IExceptionMapper mapper) where TException : Exception;
    }
}

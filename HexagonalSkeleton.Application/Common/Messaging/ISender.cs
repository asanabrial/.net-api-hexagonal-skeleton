namespace HexagonalSkeleton.Application.Common.Messaging;

/// <summary>
/// Dispatches a request to its registered handler. The application's entry point
/// into the CQRS pipeline, decoupling callers (e.g. controllers) from handlers.
/// </summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

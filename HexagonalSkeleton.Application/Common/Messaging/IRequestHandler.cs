namespace HexagonalSkeleton.Application.Common.Messaging;

/// <summary>
/// Handles a single <typeparamref name="TRequest"/> and returns a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The request being handled.</typeparam>
/// <typeparam name="TResponse">The response produced by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

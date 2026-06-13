namespace HexagonalSkeleton.Application.Common.Messaging;

/// <summary>
/// Marks a message (command or query) that is handled by a single
/// <see cref="IRequestHandler{TRequest, TResponse}"/> and produces a response.
/// </summary>
/// <typeparam name="TResponse">Type returned by the handler.</typeparam>
public interface IRequest<out TResponse>;

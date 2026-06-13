using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.Application.Common.Messaging;

/// <summary>
/// Default <see cref="ISender"/>. Resolves the matching
/// <see cref="IRequestHandler{TRequest, TResponse}"/> from the container at runtime.
/// Handler wrappers are cached per request type to keep dispatch reflection-free after the first call.
/// </summary>
internal sealed class Mediator(IServiceProvider provider) : ISender
{
    private static readonly ConcurrentDictionary<Type, HandlerWrapper> Wrappers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (HandlerWrapper<TResponse>)Wrappers.GetOrAdd(
            request.GetType(),
            static requestType => (HandlerWrapper)Activator.CreateInstance(
                typeof(HandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return wrapper.Handle(request, provider, cancellationToken);
    }

    private abstract class HandlerWrapper;

    private abstract class HandlerWrapper<TResponse> : HandlerWrapper
    {
        public abstract Task<TResponse> Handle(object request, IServiceProvider provider, CancellationToken cancellationToken);
    }

    private sealed class HandlerWrapperImpl<TRequest, TResponse> : HandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Handle(object request, IServiceProvider provider, CancellationToken cancellationToken)
        {
            var handler = provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return handler.Handle((TRequest)request, cancellationToken);
        }
    }
}

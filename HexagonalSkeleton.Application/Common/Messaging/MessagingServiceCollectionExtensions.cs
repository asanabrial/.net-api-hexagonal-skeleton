using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.Application.Common.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="ISender"/> and every <see cref="IRequestHandler{TRequest, TResponse}"/>
    /// found in the given assemblies. Replaces the previous MediatR registration.
    /// </summary>
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<ISender, Mediator>();

        var handlerImplementations = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false });

        foreach (var implementation in handlerImplementations)
        {
            var handlerInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var handlerInterface in handlerInterfaces)
                services.AddScoped(handlerInterface, implementation);
        }

        return services;
    }
}

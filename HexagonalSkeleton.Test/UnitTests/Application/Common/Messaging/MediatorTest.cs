using HexagonalSkeleton.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.Test.Unit.Application.Common.Messaging;

/// <summary>
/// Unit tests for the in-house <see cref="ISender"/> that replaced MediatR.
/// Covers handler auto-registration and runtime dispatch to the correct handler.
/// </summary>
public class MediatorTest
{
    private sealed record Ping(string Message) : IRequest<string>;

    private sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken)
            => Task.FromResult($"pong:{request.Message}");
    }

    private static ISender BuildSender()
    {
        var services = new ServiceCollection();
        services.AddRequestHandlers(typeof(MediatorTest).Assembly);
        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Send_DispatchesToMatchingHandler_AndReturnsResponse()
    {
        var sender = BuildSender();

        var result = await sender.Send(new Ping("hello"));

        Assert.Equal("pong:hello", result);
    }

    [Fact]
    public void AddRequestHandlers_RegistersSenderAndHandlers()
    {
        var services = new ServiceCollection();

        services.AddRequestHandlers(typeof(MediatorTest).Assembly);

        Assert.Contains(services, d => d.ServiceType == typeof(ISender));
        Assert.Contains(services, d => d.ServiceType == typeof(IRequestHandler<Ping, string>));
    }

    [Fact]
    public async Task Send_NullRequest_Throws()
    {
        var sender = BuildSender();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sender.Send<string>(null!));
    }
}

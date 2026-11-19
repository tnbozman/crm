using Crm.Platform.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class MediatorLoggingTests
{
    [Fact]
    public async Task Send_Logs_RequestTypeAndCorrelation_NeverThePayload()
    {
        using var capture = new CapturingLoggerProvider();
        var services = MediatorServices.Create();
        services.AddLogging(logging => logging.AddProvider(capture));
        services.AddRequestHandler<EchoQueryHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new EchoQuery("payload-marker-7f3a"), TestContext.Current.CancellationToken);

        var correlationId = scope.ServiceProvider.GetRequiredService<IMessageContext>().CorrelationId;
        capture.Lines.ShouldContain(line => line.Contains("Crm.Platform.Tests.EchoQuery", StringComparison.Ordinal)
            && line.Contains(correlationId.ToString(), StringComparison.Ordinal));
        capture.Lines.ShouldNotContain(line => line.Contains("payload-marker-7f3a", StringComparison.Ordinal));
    }
}

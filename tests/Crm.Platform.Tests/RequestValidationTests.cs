using System.ComponentModel.DataAnnotations;
using Crm.Platform.Errors;
using Crm.Platform.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class RequestValidationTests
{
    [Fact]
    public async Task Send_InvalidRequest_ThrowsRequestValidationException_WithoutCallingTheHandler()
    {
        var services = MediatorServices.Create();
        services.AddSingleton<Trace>();
        services.AddRequestHandler<RenameThingHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var exception = await Should.ThrowAsync<RequestValidationException>(() => scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new RenameThing("much too long"), TestContext.Current.CancellationToken));

        exception.Errors.Keys.ShouldBe(["Name"]);
        provider.GetRequiredService<Trace>().ShouldBeEmpty();
    }
}

public sealed record RenameThing([property: Required, MaxLength(5)] string Name) : ICommand<bool>;

internal sealed class RenameThingHandler(Trace trace) : IRequestHandler<RenameThing, bool>
{
    public Task<bool> Handle(RenameThing request, CancellationToken cancellationToken)
    {
        trace.Add("handler");
        return Task.FromResult(true);
    }
}

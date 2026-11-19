using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

/// <summary>CLAUDE.md layering rules: Controller → Service → DbContext/Models, DTOs at the HTTP boundary.</summary>
[Trait("Category", "Architecture")]
public sealed class LayeringTests
{
    [Fact]
    public void ProductionAssemblies_AreDiscovered()
    {
        CrmAssemblies.All.Select(a => a.GetName().Name).ShouldBe(["Crm.Host", "Crm.Migrator", "Crm.Platform"], ignoreOrder: true);
    }

    [Fact]
    public void Controllers_DoNotReference_DbContextOrModels() =>
        LayeringRules.ControllersReferencingDbContextOrModels(CrmAssemblies.All).ShouldBeEmpty("controllers use services and DTOs, never DbContext or Models");

    [Fact]
    public void Controllers_DoNotReference_DbContextOrModels_FlagsRogueOnly() =>
        LayeringRules.ControllersReferencingDbContextOrModels(FixtureModules.Implementations).ShouldBe(["Fixture.Rogue.Controllers.RoguesController"]);

    [Fact]
    public void Services_DoNotReference_HttpContextOrMvcTypes() =>
        LayeringRules.ServicesReferencingHttpContextOrMvc(CrmAssemblies.All).ShouldBeEmpty("services hold business rules and never touch HTTP or MVC types");

    [Fact]
    public void Services_DoNotReference_HttpContextOrMvcTypes_FlagsRogueOnly() =>
        LayeringRules.ServicesReferencingHttpContextOrMvc(FixtureModules.Implementations).ShouldBe(["Fixture.Rogue.Services.RogueAuditService"]);

    [Fact]
    public void Models_AreInternalAndNotUsedInDtosOrControllerSignatures() =>
        LayeringRules.PublicModels(CrmAssemblies.All).ShouldBeEmpty("EF models never leave their module");

    [Fact]
    public void Controllers_ReturnOnlyDtoTypes()
    {
        var offenders = CrmAssemblies.All
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => (Action: $"{t.Name}.{m.Name}", Payload: Payload(m.ReturnType))))
            .Where(x => x.Payload is not null && x.Payload.Namespace?.EndsWith(".Dtos", StringComparison.Ordinal) != true)
            .Select(x => $"{x.Action} returns {x.Payload!.FullName}")
            .ToList();

        offenders.ShouldBeEmpty("controller actions return DTOs from a Dtos namespace (or IActionResult)");
    }

    /// <summary>The body type an action returns, unwrapping Task and ActionResult; null for IActionResult and void.</summary>
    private static Type? Payload(Type type)
    {
        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Task<>) || type.GetGenericTypeDefinition() == typeof(ValueTask<>)))
        {
            return Payload(type.GetGenericArguments()[0]);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            return Payload(type.GetGenericArguments()[0]);
        }

        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            return Payload(type.GetGenericArguments()[^1]);
        }

        return type == typeof(void) || type == typeof(Task) || type == typeof(ValueTask) || typeof(IActionResult).IsAssignableFrom(type)
            ? null
            : type;
    }
}

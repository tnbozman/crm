using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Crm.SharedKernel.Modules;

/// <summary>
/// Lets module controllers be internal, like the services and DTOs they use, so the module's IModule class
/// is its only public type. Only types in module assemblies are affected; elsewhere MVC's public-only rule stands.
/// </summary>
internal sealed class ModuleControllerFeatureProvider(IReadOnlySet<Assembly> moduleAssemblies) : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo) =>
        base.IsController(typeInfo)
        || (moduleAssemblies.Contains(typeInfo.Assembly)
            && typeInfo is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false }
            && typeof(ControllerBase).IsAssignableFrom(typeInfo)
            && !typeInfo.IsDefined(typeof(NonControllerAttribute)));
}

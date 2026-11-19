namespace Crm.Platform.Messaging;

/// <summary>
/// The authorisation policy a request needs (for example <c>Policies.Finance</c>). Checked by the mediator as the
/// current user before the handler runs. A request without it still requires a signed-in user.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequiresPolicyAttribute(string policy) : Attribute
{
    public string Policy { get; } = policy;
}

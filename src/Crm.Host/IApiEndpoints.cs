namespace Crm.Host;

/// <summary>
/// Maps endpoints under <c>/api/v1</c>. Used by the test host to add throwaway endpoints;
/// replaced by <c>IModule.MapEndpoints</c> in S1-1.
/// </summary>
internal interface IApiEndpoints
{
    void Map(IEndpointRouteBuilder api);
}

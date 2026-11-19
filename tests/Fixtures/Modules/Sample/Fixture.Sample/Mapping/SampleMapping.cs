using Fixture.Sample.Dtos;
using Fixture.Sample.Models;

namespace Fixture.Sample.Mapping;

internal static class SampleMapping
{
    public static SampleResponse ToResponse(this SampleItem item) => new(item.Id, item.Name);
}

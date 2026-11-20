using Crm.__Module__.Mapping;
using Crm.__Module__.Models;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Mapping;

public sealed class ItemMappingTests
{
    [Fact]
    public void ToResponse_CopiesIdNameAndCreatedAt()
    {
        var item = new Item(Guid.CreateVersion7(), "Example", DateTimeOffset.UnixEpoch);

        var response = item.ToResponse();

        response.Id.ShouldBe(item.Id);
        response.Name.ShouldBe("Example");
        response.CreatedAt.ShouldBe(DateTimeOffset.UnixEpoch);
    }
}

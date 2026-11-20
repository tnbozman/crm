using Crm.__Module__.Models;
using Crm.SharedKernel.Errors;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Models;

public sealed class ItemTests
{
    [Fact]
    public void Constructor_WithValidName_SetsItsState()
    {
        var id = Guid.CreateVersion7();
        var createdAt = new DateTimeOffset(2026, 12, 14, 9, 0, 0, TimeSpan.Zero);

        var item = new Item(id, " Example ", createdAt);

        item.Id.ShouldBe(id);
        item.Name.ShouldBe("Example");
        item.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Constructor_WithBlankName_ThrowsRequestValidationException() =>
        Should.Throw<RequestValidationException>(() => new Item(Guid.CreateVersion7(), " ", DateTimeOffset.UnixEpoch));
}

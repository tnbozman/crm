namespace Fixture.Sample.Models;

internal sealed class SampleItem(Guid id, string name)
{
    public Guid Id { get; private set; } = id;

    public string Name { get; private set; } = name;
}

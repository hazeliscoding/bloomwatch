namespace BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

public readonly record struct WatchSpaceId(Guid Value)
{
    public static WatchSpaceId New() => new(Guid.NewGuid());
    public static WatchSpaceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

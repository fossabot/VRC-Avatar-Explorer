namespace AvatarExplorer.Core.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class InternalIdAttribute(string id) : Attribute
{
    public string Id { get; } = id;
}

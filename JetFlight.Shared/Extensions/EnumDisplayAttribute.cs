namespace JetFlight.Shared.Extensions;

[AttributeUsage(AttributeTargets.Field)]
public class EnumDisplayAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
}
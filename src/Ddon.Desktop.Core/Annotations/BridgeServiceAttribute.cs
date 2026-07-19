namespace Ddon.Desktop.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public class BridgeServiceAttribute : Attribute
{
    public string? Name { get; set; }
}

namespace Ddon.Desktop.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BridgeEventAttribute : Attribute
{
    public string? Name { get; set; }
}

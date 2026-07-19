namespace Ddon.Desktop.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class BridgeMethodAttribute : Attribute
{
    public string? Name { get; set; }
}

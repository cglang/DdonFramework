namespace Ddon.Desktop.Core.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class BridgeMethodAttribute : Attribute
{
    public string? Name { get; set; }
}

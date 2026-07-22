namespace Test.Desktop.Avalonia.Services;

public class PlcWriteRequest
{
    public string Address { get; set; } = "";
    public object? Value { get; set; }
}

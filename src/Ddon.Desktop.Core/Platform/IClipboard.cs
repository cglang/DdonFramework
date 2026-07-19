namespace Ddon.Desktop.Platform;

public interface IClipboard
{
    Task<string> GetTextAsync();
    Task SetTextAsync(string text);
}

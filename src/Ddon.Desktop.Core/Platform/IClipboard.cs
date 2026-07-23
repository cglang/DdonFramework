namespace Ddon.Desktop.Core.Platform;

public interface IClipboard
{
    Task<string> GetTextAsync();
    Task SetTextAsync(string text);
}

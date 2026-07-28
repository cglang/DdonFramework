namespace VitrinRuntime.Desktop.Exceptions;

public sealed class UserFriendlyException : Exception
{
    public UserFriendlyException(string message) : base(message) { }
}

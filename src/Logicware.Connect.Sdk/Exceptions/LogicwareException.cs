namespace Logicware.Connect.Sdk.Exceptions;

/// <summary>
/// Base class for every error thrown by the Logicware Connect SDK.
/// </summary>
public class LogicwareException : Exception
{
    public LogicwareException(string message) : base(message) { }
    public LogicwareException(string message, Exception inner) : base(message, inner) { }
}

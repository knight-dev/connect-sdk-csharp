namespace Logicware.Connect.Sdk.Exceptions;

/// <summary>
/// Thrown when the transport layer fails: socket error, DNS failure, TLS
/// failure, timeout, or network unreachable. The inner exception is always
/// the original <c>HttpRequestException</c> / <c>TaskCanceledException</c>.
/// </summary>
public class LogicwareNetworkException : LogicwareException
{
    public LogicwareNetworkException(string message, Exception inner)
        : base(message, inner) { }
}

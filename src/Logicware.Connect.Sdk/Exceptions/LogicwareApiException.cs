namespace Logicware.Connect.Sdk.Exceptions;

/// <summary>
/// Thrown when the API returns a non-2xx status that the SDK has stopped
/// retrying on. Carries the full machine-readable error shape so callers can
/// branch on <see cref="ErrorCode"/> / <see cref="Status"/> without re-parsing.
/// </summary>
public class LogicwareApiException : LogicwareException
{
    /// <summary>HTTP status code as returned by the server (e.g., 400, 422).</summary>
    public int Status { get; }

    /// <summary>
    /// Machine-readable code from the response body when present
    /// (e.g., <c>"ADDRESS_CODE_REQUIRED"</c>). Null when the API returned an
    /// unstructured error.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary><c>X-Request-Id</c> echoed by the server, if any.</summary>
    public string? RequestId { get; }

    /// <summary>
    /// Raw parsed body (dictionary / list) or, when the body wasn't JSON,
    /// the response body as a string. Use this for surfacing validation-
    /// error arrays and the like.
    /// </summary>
    public object? Details { get; }

    public LogicwareApiException(
        string message,
        int status,
        string? errorCode,
        string? requestId,
        object? details) : base(message)
    {
        Status = status;
        ErrorCode = errorCode;
        RequestId = requestId;
        Details = details;
    }
}

using CSM_Foundation_Core.Errors.Models;

namespace CSM_Server_Core.Core.Models;

/// <summary>
///     Represents public tradeable internal errors information for public error exposure.
/// </summary>
public class ErrorInfo {

    /// <summary>
    ///     Error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    ///     Error advise message.
    /// </summary>
    public required string Advise { get; init; }

    /// <summary>
    ///     Error event trigger.
    /// </summary>
    public required int Event { get; init; }


    /// <summary>
    ///     System error message.
    /// </summary>
    public required string? SystemError { get; init; }

    /// <summary>
    ///     Error user feedback.
    /// </summary>
    public ErrorFeedback[] Feedback = [];

    /// <summary>
    ///     Error analysis data.
    /// </summary>
    public IDictionary<string, object?> Data { get; init; } = new Dictionary<string, object?>();
}

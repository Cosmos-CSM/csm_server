using System.Net;
using System.Text.Json.Serialization;

using CSM_Foundation_Core.Errors.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents an error from a server side.
/// </summary>
public interface IServerError
    : IError {

    /// <summary>
    ///     Generic event object value.
    /// </summary>
    public object Event { get; }

    /// <summary>
    ///     Whether the server error was triggered by a foundation CSM framework error.
    /// </summary>
    public IError? Error { get; }

    /// <summary>
    ///     Indicates a custom status code for the transaction resolution.
    /// </summary>
    public HttpStatusCode Status { get; }

    [JsonIgnore]
    public new Exception? Exception { get; }
}

/// <inheritdoc cref="IServerError"/>
/// <typeparam name="TEvents">
///     Error events types.
/// </typeparam>
public interface IServerError<TEvents>
    : IServerError, IException<TEvents>
    where TEvents : Enum {
}
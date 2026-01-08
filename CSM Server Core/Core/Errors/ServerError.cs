using System.Text.Json.Serialization;

using CSM_Foundation_Core.Errors.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Bases;

namespace CSM_Server_Core.Core.Errors;

/// <summary>
///     Represents events triggers for <see cref="ServerError"/>.
/// </summary>
public enum ServerErrorEvents {
    /// <summary>
    ///     Trigger when an inner error was caught and is being wrapped. 
    /// </summary>
    WRAP_ERROR,
}

/// <inheritdoc cref="ServerErrorBase{TEvents}"/>
public class ServerError
    : ServerErrorBase<ServerErrorEvents> {

    [JsonIgnore]
    new Exception? Exception { get; }

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="event">
    ///     Error trigger event.
    /// </param>
    /// <param name="error">
    ///     CSM Framework error.
    /// </param>
    /// <param name="exception">
    ///     Native .Net exception.
    /// </param>
    public ServerError(ServerErrorEvents @event = ServerErrorEvents.WRAP_ERROR, IError? error = null, Exception? exception = null)
        : base("Server error", @event, error, exception) {
    }
}

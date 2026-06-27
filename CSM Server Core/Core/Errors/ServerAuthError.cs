using System.Net;

using CSM_Server_Core.Abstractions.Bases;

namespace CSM_Server_Core.Core.Errors;

/// <summary>
///     Represents <see cref="ServerAuthError"/> events.
/// </summary>
public enum ServerAuthErrorEvents {
    /// <summary>
    ///     Triggered when server authentication can't find the controller requested.
    /// </summary>
    UNFOUND_CONTROLLER,

    /// <summary>
    ///     Triggered when the action attribute is on a controller with no feature attribute.
    /// </summary>
    UNFOUND_FEATURE,

    /// <summary>
    ///     Triggered when the current user can't act at the requested feature / action / solution context.
    /// </summary>
    UNAUTH,
}


/// <summary>
///     Represents a server authentication operation errror.
/// </summary>
public class ServerAuthError
    : ServerErrorBase<ServerAuthErrorEvents> {

    /// <summary>
    ///     Creates a new instance
    /// </summary>
    /// <param name="event">
    ///     Error event.
    /// </param>
    public ServerAuthError(ServerAuthErrorEvents @event)
        : base("Server auth error", @event, statusCode: HttpStatusCode.Unauthorized) {
    }
}

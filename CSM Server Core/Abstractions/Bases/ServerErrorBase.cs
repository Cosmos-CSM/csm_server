
using System.Net;

using CSM_Foundation_Core.Errors.Abstractions.Bases;
using CSM_Foundation_Core.Errors.Abstractions.Interfaces;
using CSM_Foundation_Core.Errors.Models;

using CSM_Server_Core.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Bases;

/// <inheritdoc cref="IServerError"/>
/// <typeparam name="TEvents">
///     Error trigger events.
/// </typeparam>
public class ServerErrorBase<TEvents>
    : ErrorBase<TEvents>, IServerError<TEvents>
    where TEvents : Enum {

    public IError? Error { get; }

    public HttpStatusCode Status { get; }

    object IServerError.Event { get => Convert.ToInt32(Event); }
    

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="message">
    ///     Error title message.
    /// </param>
    /// <param name="event">
    ///     Error event trigger.
    /// </param>
    /// <param name="exception">
    ///     System exception caught.
    /// </param>
    /// <param name="statusCode">
    ///     HTTP status code identification.
    /// </param>
    /// <param name="feedback">
    ///     User's friendly feedback data.
    /// </param>
    /// <param name="data">
    ///     Analysis purposes exception data.
    /// </param>
    public ServerErrorBase(string message, TEvents @event, IError? error = null, Exception? exception = null, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, ErrorFeedback[]? feedback = null, IDictionary<string, object?>? data = null)
        : base(message, @event, exception, feedback, data) {

        Error = error;
        Status = statusCode;
    }
}

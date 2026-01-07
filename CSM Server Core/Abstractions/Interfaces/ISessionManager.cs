using Microsoft.AspNetCore.Http;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents a server session manager service.
/// </summary>
public interface ISessionManager {

    /// <summary>
    ///     Validates if the current user has access to the action.
    /// </summary>
    /// <param name="featureName">
    ///     Feature the action is being trying to be called.
    /// </param>
    /// <param name="actionName">
    ///     Action name to be performed.
    /// </param>
    /// <param name="httpContext">
    ///     Current request context.
    /// </param>
    /// <returns>
    ///     Whether the action can be performed or not.
    /// </returns>
    bool ValidateUserAction(string featureName, string actionName, HttpContext httpContext);
}

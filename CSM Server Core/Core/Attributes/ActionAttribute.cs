using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Errors;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CSM_Server_Core.Core.Attributes;

/// <summary>
///     Attributes that defines a controller method as a secured business action and validates if the current user has access to it.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ActionAttribute
    : Attribute, IAuthorizationFilter {

    /// <summary>
    ///     Action that specifies the permit.
    /// </summary>
    readonly string Action;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="actionName"></param>
    public ActionAttribute(string actionName) {
        Action = actionName;
    }

    /// <inheritdoc/>
    public void OnAuthorization(AuthorizationFilterContext context) {
        HttpContext reqContext = context.HttpContext;

        // Resolve session manager
        ISessionManager sessionManager = reqContext.RequestServices.GetRequiredService<ISessionManager>();

        IList<object> endpointMetaData = context.ActionDescriptor.EndpointMetadata;
        FeatureAttribute featureAttribute = endpointMetaData.OfType<FeatureAttribute>().FirstOrDefault()
            ?? throw new ServerAuthError(ServerAuthErrorEvents.UNFOUND_FEATURE);

        string featureName = featureAttribute.Feature;

        // Now validate Feature + Action
        bool canAct = sessionManager.ValidateUserAction(featureName, Action, reqContext);

        if (!canAct) {
            throw new ServerAuthError(ServerAuthErrorEvents.UNAUTH);
        }
    }
}
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

    public void OnAuthorization(AuthorizationFilterContext context) {
        HttpContext reqContext = context.HttpContext;

        // Resolve session manager
        var sessionManager = reqContext.RequestServices.GetRequiredService<ISessionManager>();

        // Get the controller type from the ActionDescriptor
        var controllerType = (context.ActionDescriptor
            .EndpointMetadata
            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
            .FirstOrDefault()
            ?.ControllerTypeInfo)
            ?? throw new ServerAuthError(ServerAuthErrorEvents.UNFOUND_CONTROLLER);

        // Read the FeatureAttribute from the controller

        if (controllerType
            .GetCustomAttributes(typeof(FeatureAttribute), inherit: true)
            .FirstOrDefault() is not FeatureAttribute featureAttr) {
            throw new ServerAuthError(ServerAuthErrorEvents.UNFOUND_FEATURE);
        }

        string featureName = featureAttr.Feature;

        // Now validate Feature + Action
        bool canAct = sessionManager.ValidateUserAction(featureName, Action, reqContext);

        if (!canAct) {
            throw new ServerAuthError(ServerAuthErrorEvents.UNAUTH);
        }
    }
}
using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Errors;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CSM_Server_Core.Middlewares;

/// <summary>
///     Represents a middleware responsible for wipe out quality and testing data.
/// </summary>
public class DispositionMiddleware
    : IMiddleware {

    const string DISP_HEAD_KEY = "CSMDisposition";
    const string DISP_HEAD_VALUE = "Quality";

    readonly IServerDisposer _disposer;

    /// <summary>
    ///     Creates a new intsance.
    /// </summary>
    /// <param name="disposer">
    ///     Server disposer handler.
    /// </param>
    public DispositionMiddleware(IServerDisposer disposer) {
        _disposer = disposer;
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        HttpRequest request = context.Request;

        StringValues headers = request.Headers[DISP_HEAD_KEY];

        bool Activate = false;
        if (headers.Count > 0) {
            if (!headers.Contains(DISP_HEAD_VALUE)) {
                throw new DispositionError(XDispositionSituations.WRONG_TOKEN);
            }

            Activate = true;
        }

        _disposer.ChangeState(Activate);
        await next(context);
    }
}
using CSM_Server_Core.Core.Errors;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CSM_Server_Core.Core.Models.Frames;

public class DispositionMiddleware
    : IMiddleware {

    const string DISP_HEAD_KEY = "CSMDisposition";
    const string DISP_HEAD_VALUE = "Quality";

    readonly ServerDisposer _disposer;

    public DispositionMiddleware(ServerDisposer disposer) {
        _disposer = disposer;
    }

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
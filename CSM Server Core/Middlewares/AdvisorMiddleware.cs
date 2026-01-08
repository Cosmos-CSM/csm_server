
using System.Text.Json;

using CSM_Foundation_Core.Core.Utils;
using CSM_Foundation_Core.Errors.Abstractions.Interfaces;

using Microsoft.AspNetCore.Http;

using JObject = System.Collections.Generic.Dictionary<string, object?>;

namespace CSM_Server_Core.Middlewares;

/// <summary>
///     Represents a server middleware that handles console logging information through requests.
/// </summary>
public class AdvisorMiddleware
    : IMiddleware {

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        try {
            ConsoleUtils.Announce(
                $"Request received",
                new() {
                    { "Tracer", context.TraceIdentifier },
                    { "Address", $"({context.Connection.RemoteIpAddress}:{context.Connection.RemotePort})" },
                }
            );

            Stream originalStream = context.Response.Body;
            await next(context);

            HttpResponse Response = context.Response;
            if (!Response.HasStarted) {
                Stream bufferStream = Response.Body;
                JObject? responseContent = await JsonSerializer.DeserializeAsync<JObject>(bufferStream);
                
                if (responseContent != null && responseContent.TryGetValue("Details", out dynamic? value)) {
                    JsonElement Estela = value;
                    JObject? EstelaObject = Estela.Deserialize<JObject>();
                    if (EstelaObject != null && EstelaObject.ContainsKey("Failure")) {
                        ConsoleUtils.Warning($"Reques served with failure", responseContent);
                    } else {
                        ConsoleUtils.Success($"Request served successful", responseContent);
                    }
                } else if (Response.StatusCode != 204) {
                    ConsoleUtils.Success($"Request served successful", responseContent);
                }

                if (Response.StatusCode != 204) {
                    _ = bufferStream.Seek(0, SeekOrigin.Begin);
                    await bufferStream.CopyToAsync(originalStream);
                    Response.Body = originalStream;
                }
            }
        } catch (Exception ex) when (ex is IError error) {
            ConsoleUtils.Exception(error);
            throw;
        } catch (Exception ex) {
            ConsoleUtils.Warning(
                    "Exception at Advisor middleware with no ancestor",
                    new JObject {
                        { "Message", ex.Message },
                        { "StackTrace", ex.StackTrace }
                    }
                );
            throw;
        }
    }
}

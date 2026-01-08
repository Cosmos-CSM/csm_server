using System.Net;
using System.Text.Json;

using CSM_Foundation_Core.Core.Exceptions;
using CSM_Foundation_Core.Errors.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Errors;
using CSM_Server_Core.Core.Models.Frames;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace CSM_Server_Core.Middlewares;

/// <summary>
///     Represents a server middleware that bootstraps errors and wrong formatted responses to convert them into business known data.
/// </summary>
public class FramingMiddleware
    : IMiddleware {

    /// <summary>
    /// 
    /// </summary>
    const string DEF_CONTENT_TYPE = "application/json";

    /// <summary>
    ///     Action call to initialize top level databases transactions.
    /// </summary>
    readonly Func<IServiceProvider, Task> _transactionsInitializer;

    /// <summary>
    ///     Action call to commit top level databases transaction changes.
    /// </summary>
    readonly Func<IServiceProvider, Task> _transactionsCommitter;

    /// <summary>
    ///     Action call to rollback database changes when errors found.
    /// </summary>
    readonly Func<IServiceProvider, Task> _transactionsRollback;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="initTransactions">
    ///     Call delegator to start database context transactions.
    /// </param>
    /// <param name="commitTransactions">
    ///     Call delegator to commit database changes during request scope transactions.
    /// </param>
    /// <param name="rollBackTransactions">
    ///     Action call to rollback database changes when errors found.
    /// </param>
    public FramingMiddleware(Func<IServiceProvider, Task> initTransactions, Func<IServiceProvider, Task> commitTransactions, Func<IServiceProvider, Task> rollBackTransactions) {
        _transactionsInitializer = initTransactions;
        _transactionsCommitter = commitTransactions;
        _transactionsRollback = rollBackTransactions;
    }

    /// <summary>
    ///     Converts the given <paramref name="serverError"/> data to a content generic object.
    /// </summary>
    /// <param name="serverError">
    ///     Error data to convert.
    /// </param>
    /// <returns>
    ///     Generic error content data object.
    /// </returns>
    static Dictionary<string, object?> ConvertErrorContent(IServerError serverError) {
        string errorTextContent = JsonSerializer.Serialize(serverError);

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(errorTextContent)!;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        if (!Guid.TryParse(context.TraceIdentifier, out Guid Tracer)) {
            Tracer = Guid.NewGuid();
            context.TraceIdentifier = Tracer.ToString();
        }

        // --> Starting database transactions.
        await _transactionsInitializer(context.RequestServices);

        IServerError? error = null;
        using MemoryStream reqProxyBuffer = new();

        try {
            context.Response.Body = reqProxyBuffer;

            await next.Invoke(context);

            // --> Commit database transactions.
            await _transactionsCommitter(context.RequestServices);
        } catch (Exception ex) when (ex is IError exError) {
            error = new ServerError(error: exError);

            /// --> Rollbacking database changes
            await _transactionsRollback(context.RequestServices);
        } catch (Exception ex) when (ex is IServerError serverError) {
            error = serverError;

            /// --> Rollbacking database changes
            await _transactionsRollback(context.RequestServices);
        } catch (Exception ex) {
            ServerError systemError = new(
                exception: ex, 
                error: new SystemError(ex.Message)
            );
            error = systemError;

            /// --> Rollbacking database changes
            await _transactionsRollback(context.RequestServices);
        } finally {
            HttpResponse response = context.Response;
            Stream responseStream = response.Body;

            if (!response.HasStarted) {
                reqProxyBuffer.Seek(0, SeekOrigin.Begin);
                string encodedContent = "";
                if (error is not null) {
                    Dictionary<string, object?> errorContent = ConvertErrorContent(error);

                    FailureFrame frame = new() {
                        Id = Tracer,
                        Content = errorContent,
                    };

                    response.StatusCode = (int)error.Status;
                    encodedContent = JsonSerializer.Serialize(frame);
                } else if (response.StatusCode != 200) {

                    switch (response.StatusCode) {
                        case 204: {
                                SuccessFrame<Dictionary<string, object?>?> frame = new() {
                                    Id = Tracer,
                                    Content = null,
                                };

                                response.StatusCode = (int)HttpStatusCode.OK;
                                encodedContent = JsonSerializer.Serialize(frame);
                            }
                            break;
                        case 405: {
                                error = new ServerError(
                                        error: new SystemError("Unsupported HTTP Method")
                                    );
                                Dictionary<string, object?> errorContent = ConvertErrorContent(error);

                                FailureFrame frame = new() {
                                    Id = Tracer,
                                    Content = errorContent,
                                };
                                encodedContent = JsonSerializer.Serialize(frame);
                            }
                            break;
                        case 404: {
                                error = new ServerError(
                                        error: new SystemError($"{context.Request.GetDisplayUrl()} not found")
                                    );
                                Dictionary<string, object?> errorContent = ConvertErrorContent(error);

                                FailureFrame frame = new() {
                                    Id = Tracer,
                                    Content = errorContent,
                                };

                                encodedContent = JsonSerializer.Serialize(frame);
                            }
                            break;
                        default:
                            Dictionary<string, object> jObject = JsonSerializer.Deserialize<Dictionary<string, object>>(responseStream)!;

                            SuccessFrame<Dictionary<string, dynamic>> defFrame = new() {
                                Id = Tracer,
                                Content = jObject,
                            };
                            encodedContent = JsonSerializer.Serialize(defFrame);
                            break;
                    }
                } else {

                    Dictionary<string, dynamic> resolution = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(responseStream)!;

                    SuccessFrame<Dictionary<string, dynamic>> frame = new() {
                        Id = Tracer,
                        Content = resolution,
                    };

                    response.StatusCode = (int)HttpStatusCode.OK;
                    encodedContent = JsonSerializer.Serialize(frame);
                }

                response.ContentType = DEF_CONTENT_TYPE;


                MemoryStream swapperBuffer = new();
                StreamWriter writer = new(swapperBuffer);

                await writer.WriteAsync(encodedContent);
                await writer.FlushAsync();

                swapperBuffer.Seek(0, SeekOrigin.Begin);
                response.Body = swapperBuffer;
            }
        }
    }
}
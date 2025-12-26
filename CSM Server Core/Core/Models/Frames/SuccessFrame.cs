using CSM_Server_Core.Abstractions.Interfaces;

namespace CSM_Server_Core.Core.Models.Frames;

/// <summary>
///     Represents a server response success frame data.
/// </summary>
/// <typeparam name="TSuccess">
///     Type of the success content data.
/// </typeparam>
public class SuccessFrame<TSuccess>
    : IResponseSchema<TSuccess> {

    public required Guid Id { get; init; }

    public required TSuccess Content { get; init; }
}

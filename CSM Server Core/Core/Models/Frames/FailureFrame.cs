using CSM_Server_Core.Abstractions.Interfaces;

namespace CSM_Server_Core.Core.Models.Frames;

/// <summary>
///     Represents a failure server frame response.
/// </summary>
public class FailureFrame
    : IResponseSchema<Dictionary<string, object?>> {

    public required Guid Id { get; init; }

    public required Dictionary<string, object?> Content { get; init; }
}

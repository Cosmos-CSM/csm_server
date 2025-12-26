using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents a server disposer manager to handle temporary data.
/// </summary>
public interface IServerDisposer
    : IDisposer<IEntity> {

    /// <summary>
    ///     Changes the manager state.
    /// </summary>
    /// <param name="active">
    ///     Wheter the manager must be tracking data or not.
    /// </param>
    public void ChangeState(bool active);
}

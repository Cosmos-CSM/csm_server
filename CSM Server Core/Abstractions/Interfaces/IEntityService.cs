using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Core.Models;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents a server service scoped operations.
/// </summary>
public interface IService {
}

/// <summary>
///     Represents a server service scoped to an <see cref="IEntity"/>
/// </summary>
/// <typeparam name="TEntity">
///     Type of the scoped <see cref="IEntity"/>.
/// </typeparam>
public interface IEntityService<TEntity>
    : IService
    where TEntity : class, IEntity {

    /// <summary>
    ///     Generates a complex data [View], works as a complex paginated query to build tables or 
    ///     analyze entity data.
    /// </summary>
    /// <param name="input">
    ///     View input.
    /// </param>
    /// <returns>
    ///     View output.
    /// </returns>
    public Task<ViewOutput<TEntity>> View(EntityServiceInput<ViewInput<TEntity>> input);
}
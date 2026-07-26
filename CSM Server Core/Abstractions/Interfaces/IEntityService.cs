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
    ///     Updates a <typeparamref name="TEntity"/> data.
    /// </summary>
    /// <param name="input">
    ///     Service input.
    /// </param>
    /// <returns>
    ///     Service output.
    /// </returns>
    public Task<UpdateOutput<TEntity>> Update(EntityServiceInput<UpdateInput<TEntity>> input);

    /// <summary>
    ///     Generates a complex data [View], works as a complex paginated query to build tables or 
    ///     analyze entity data.
    /// </summary>
    /// <param name="input">
    ///     Service input.
    /// </param>
    /// <returns>
    ///     Service output.
    /// </returns>
    public Task<ViewOutput<TEntity>> View(EntityServiceInput<ViewInput<TEntity>> input);

    /// <summary>
    ///     Creates a batch of <typeparamref name="TEntity"/> objects.
     /// </summary>
    /// <param name="input">
    ///     Service input.
    /// </param>
    /// <returns>
    ///     Service output.
    /// </returns>
    public Task<BatchOperationOutput<TEntity>> Create(TEntity[] input);
}
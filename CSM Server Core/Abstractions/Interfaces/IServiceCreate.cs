using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents creation base operations from a <see cref="IEntity"/> scoped server service.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> service scope.
/// </typeparam>
public interface IServiceCreate<TEntity>
    : IService
    where TEntity : class, IEntity {

    /// <summary>
    ///     Creates the given <paramref name="entity"/> into data storages.
    /// </summary>
    /// <param name="entity">
    ///     <see cref="IEntity"/> to create.
    /// </param>
    /// <returns>
    ///     Store <see cref="IEntity"/> data.
    /// </returns>
    Task<TEntity> Create(TEntity entity);

    /// <summary>
    ///     Creates the given <paramref name="entities"/> collection into data storages.
    /// </summary>
    /// <param name="entities">
    ///     Collection of <see cref="IEntity"/> to create.
    /// </param>
    /// <param name="sync">
    ///     Whether the operation must finish at the first error, throwing instantly an exception.
    /// </param>
    /// <returns>
    ///    Output data.
    /// </returns>
    /// <remarks>
    ///     Property <see cref="IEntity.Timestamp"/> is always overriden before saving the data to 
    ///     set the <see cref="DateTime.UtcNow"/> in order to get the most accurate creation timemark.
    /// </remarks>
    Task<BatchOperationOutput<TEntity>> Create(TEntity[] entities, bool sync = false);
}

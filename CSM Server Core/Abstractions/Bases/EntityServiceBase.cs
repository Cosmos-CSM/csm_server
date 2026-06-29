using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Models;

namespace CSM_Server_Core.Abstractions.Bases;

/// <summary>
///     Represents a business <see cref="IEntity"/> service.
/// </summary>
/// <typeparam name="TEntity">
///     <see cref="IEntity"/> implementation type the service handles.
/// </typeparam>
/// <typeparam name="TDepot">
///     <see cref="IDepot{TEntity}"/> implementation type the service's entity handling type is based on.
/// </typeparam>
public abstract class EntityServiceBase<TEntity, TDepot>
    : IEntityService<TEntity>
    where TEntity : class, IEntity
    where TDepot : IDepot<TEntity> {

    /// <summary>
    ///    Service entity type hadling depot.
    /// </summary>
    protected readonly TDepot _depot;


    /// <summary>
    ///     Creates a new instance. 
    /// </summary>
    /// <param name="depot">
    ///     Entity type depot handler.
    /// </param>
    public EntityServiceBase(TDepot depot) {
        _depot = depot;
    }

    /// <inheritdoc/>
    public async Task<ViewOutput<TEntity>> View(EntityServiceInput<ViewInput<TEntity>> input) {
        IEnumerable<string> relations = input.Relations ?? [];
        relations = relations.Where(
                relation => !string.IsNullOrWhiteSpace(relation)
            );



        return await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = input.Parameters,
                }
            );
    }
}

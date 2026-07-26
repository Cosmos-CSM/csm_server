using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Models;
using CSM_Server_Core.Core.Utils.Abstractions.Interfaces;

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
    ///     Entity Service common utils.
    /// </summary>
    protected readonly IEntityServiceUtils _eServiceUtils;

    /// <summary>
    ///     Creates a new instance. 
    /// </summary>
    /// <param name="depot">
    ///     Entity type depot handler.
    /// </param>
    /// <param name="entityServiceUtils">
    ///     Entity Servcice Utilities depdendency.
    /// </param>
    public EntityServiceBase(TDepot depot, IEntityServiceUtils entityServiceUtils) {
        _depot = depot;
        _eServiceUtils = entityServiceUtils;
    }

    /// <inheritdoc/>
    public async Task<BatchOperationOutput<TEntity>> Create(TEntity[] input) {
        return await _depot.Create(input);
    }

    /// <inheritdoc/>
    public async Task<UpdateOutput<TEntity>> Update(EntityServiceInput<UpdateInput<TEntity>> input) {
        QueryProcessor<TEntity> relationsProcessor = _eServiceUtils.IncludeRelations<TEntity>(input.Relations);
 
        return await _depot.Update(
                new QueryInput<TEntity, UpdateInput<TEntity>> {
                    Parameters = input.Parameters,
                    PostProcessor = relationsProcessor
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ViewOutput<TEntity>> View(EntityServiceInput<ViewInput<TEntity>> input) {
        // Here we calculate relations and applied to the query.
        QueryProcessor<TEntity> qryProcessor = _eServiceUtils.IncludeRelations<TEntity>(input.Relations);

        return await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = input.Parameters,
                    PostProcessor = qryProcessor,
                    PreProcessor = qryProcessor,
                }
            );
    }
}

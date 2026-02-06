using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Bases;

/// <summary>
///     Represents a business operations service.
/// </summary>
/// <typeparam name="TEntity">
///     <see cref="IEntity"/> implementation type the service handles.
/// </typeparam>
/// <typeparam name="TDepot">
///     <see cref="IDepot{TEntity}"/> implementation type the service's entity handling type is based on.
/// </typeparam>
public abstract class ServiceBase<TEntity, TDepot>
    : IService<TEntity>
    where TEntity : class, IEntity
    where TDepot : IDepot<TEntity> {

    /// <summary>
    ///    Service entity type hadling depot.
    /// </summary>
    protected readonly TDepot _depot;

    /// <summary>
    ///     Global service scope pre query process operation applied to all service's operations.
    /// </summary>
    readonly QueryProcessor<TEntity>? _preProcessor;

    /// <summary>
    ///     Global service scope post query process operation applied to all service's operations.
    /// </summary>
    readonly QueryProcessor<TEntity>? _postProcessor;


    /// <summary>
    ///     Creates a new instance. 
    /// </summary>
    /// <param name="depot">
    ///     Entity type depot handler.
    /// </param>
    /// <param name="preProcessor">
    ///     Global service scope pre query process operation applied to all service's operations.
    /// </param>
    /// <param name="postProcessor">
    ///     Global service scope post query process operation applied to all service's operations.
    /// </param>
    public ServiceBase(TDepot depot, QueryProcessor<TEntity>? preProcessor = null, QueryProcessor<TEntity>? postProcessor = null) {
        _depot = depot;
        _preProcessor = preProcessor;
        _postProcessor = postProcessor;
    }

    /// <inheritdoc/>
    public virtual Task<ViewOutput<TEntity>> View(ViewInput<TEntity> input)
    => _depot.View(
            GetOperationInput(
                    new QueryInput<TEntity, ViewInput<TEntity>> {
                        Parameters = input,
                    }
                )
        );

    /// <inheritdoc/>
    public virtual Task<TEntity> Create(TEntity entity)
    => _depot.Create(entity);

    /// <inheritdoc/>
    public virtual Task<BatchOperationOutput<TEntity>> Create(TEntity[] entities, bool sync = false)
    => _depot.Create(entities, sync);

    /// <inheritdoc/>
    public virtual Task<UpdateOutput<TEntity>> Update(UpdateInput<TEntity> input)
    => _depot.Update(
            GetOperationInput(
                    new QueryInput<TEntity, UpdateInput<TEntity>> {
                        Parameters = input,
                    }
                )
        );

    /// <inheritdoc/>
    public virtual Task<TEntity> Delete(long id)
    => _depot.Delete(id);

    /// <inheritdoc/>
    public virtual Task<TEntity> Delete(TEntity entity)
    => _depot.Delete(entity);

    /// <inheritdoc/>
    public virtual Task<BatchOperationOutput<TEntity>> Delete(long[] ids)
    => _depot.Delete(ids);

    /// <inheritdoc/>
    public virtual Task<BatchOperationOutput<TEntity>> Delete(TEntity[] entities)
    => _depot.Delete(entities);

    /// <summary>
    ///     Gets the default <see cref="ServiceBase{TEntity, TDepot}"/> query input.
    /// </summary>
    /// <typeparam name="TParameters"></typeparam>
    /// <param name="serviceQueryInput"></param>
    /// <returns></returns>
    protected QueryInput<TEntity, TParameters> GetOperationInput<TParameters>(QueryInput<TEntity, TParameters> serviceQueryInput)
    => new() {
        Parameters = serviceQueryInput.Parameters,
        PreProcessor = serviceQueryInput.PreProcessor ?? _preProcessor,
        PostProcessor = serviceQueryInput.PostProcessor ?? _postProcessor,
    };
}

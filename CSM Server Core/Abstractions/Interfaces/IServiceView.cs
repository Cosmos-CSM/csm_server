using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents view base operations from a <see cref="IEntity"/> scoped server service.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> service scope.
/// </typeparam>
public interface IServiceView<TEntity>
    : IService
    where TEntity : class, IEntity {

    /// <summary>
    ///     Generates a <typeparamref name="TEntity"/> view,
    /// </summary>
    /// <param name="input">
    ///     <see cref="ViewInput{T}"/> data.
    /// </param>
    /// <returns>
    ///     <see cref="ViewOutput{TEntity}"/> data.
    /// </returns>
    Task<ViewOutput<TEntity>> View(ViewInput<TEntity> input);
}

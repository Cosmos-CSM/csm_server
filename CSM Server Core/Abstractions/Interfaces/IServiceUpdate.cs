using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents update base operations from a <see cref="IEntity"/> scoped server service.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> service scope.
/// </typeparam>
public interface IServiceUpdate<TEntity>
    : IService
    where TEntity : class, IEntity {

    /// <summary>
    ///     Updates the given <see cref="UpdateInput{TEntity}.Entity"/> based on the <paramref name="input"/> params.
    /// </summary>
    /// <param name="input">
    ///     <see cref="UpdateInput{TEntity}"/> data.
    /// </param>
    /// <returns>
    ///     <see cref="UpdateOutput{T}"/> data.
    /// </returns>
    Task<UpdateOutput<TEntity>> Update(UpdateInput<TEntity> input);
}

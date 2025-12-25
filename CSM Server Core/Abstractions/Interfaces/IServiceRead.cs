using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents read base operations from a <see cref="IEntity"/> scoped server service.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> service scope.
/// </typeparam>
public interface IServiceRead<TEntity>
    : IService
    where TEntity : class, IEntity {
}

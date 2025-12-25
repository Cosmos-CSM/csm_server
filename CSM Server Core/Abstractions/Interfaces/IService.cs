using CSM_Database_Core.Entities.Abstractions.Interfaces;

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
public interface IService<TEntity>
    : IService,
    IServiceView<TEntity>,
    IServiceRead<TEntity>,
    IServiceUpdate<TEntity>,
    IServiceCreate<TEntity>,
    IServiceDelete<TEntity>
    where TEntity : class, IEntity {
}
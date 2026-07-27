using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Server_Core.Core.Utils.Abstractions.Interfaces;

/// <summary>
///     Provides utility methods for business entities scoped service operations.
/// </summary>
public interface IEntityServiceUtils {

    /// <summary>
    ///     Includes given <paramref name="relations"/> to the query delegator returned.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     Type of the <see cref="IEntity"/> being handled.
    /// </typeparam>
    /// <returns>
    ///     Query delegator with applied relations.
    /// </returns>
    QueryProcessor<TEntity> IncludeRelations<TEntity>(string[] relations)
        where TEntity : class, IEntity;
}

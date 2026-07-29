using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Core.Utils.Abstractions.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace CSM_Server_Core.Core.Utils;

/// <summary>
///     Provides utilities method for business entities based services operations.
/// </summary>
public class EntityServiceUtils : IEntityServiceUtils {
    
    /// <inheritdoc/>
    public QueryProcessor<TEntity> IncludeRelations<TEntity>(string[] relations)
        where TEntity : class, IEntity {
        IEnumerable<string> rels = relations ?? [];
        rels = rels.Where(
                rel => !string.IsNullOrWhiteSpace(rel)
            );

        return (srcQuery) => {
            foreach (string rel in rels) {
                srcQuery = srcQuery.Include(rel);
            }

            return srcQuery;
        };
    }
}

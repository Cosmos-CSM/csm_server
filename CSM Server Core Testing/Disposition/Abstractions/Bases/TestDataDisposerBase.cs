using System.Collections.Concurrent;

using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Abstractions.Interfaces;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CSM_Server_Core_Testing.Disposition.Abstractions.Bases;

/// <summary>
///     Public Delegate for [database] factory [Quality] purposes.
/// </summary>
/// <returns>
///     The database context instance.
/// </returns>
public delegate DbContext DatabaseFactory();

/// <summary>
///     Represents a tests data disposer, used on integration tests that stores testing data on databases,
///     to ensure these testing data items are removed after testing finishes.
/// </summary>
public abstract class TestDataDisposerBase
    : IDisposer<IEntity> {

    /// <summary>
    ///     Current [Disposer] Database factories available.  
    /// </summary>
    protected Dictionary<Type, DatabaseFactory> Factories { get; private init; } = [];

    /// <summary>
    ///     Current [Disposer] queue entities to dispose related with their databases owners.
    /// </summary>
    protected ConcurrentDictionary<Type, IEntity[]> Queue { get; private init; } = [];

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="Factories">
    ///     Database factories where all the data to dispose belong to, useful for
    ///     cross database, and multi database solutions.
    /// </param>
    public TestDataDisposerBase(params DatabaseFactory[] Factories) {
        foreach (DatabaseFactory Factory in Factories) {
            using DbContext instance = Factory();

            Type dbType = instance.GetType();
            this.Factories.Add(dbType, Factory);
            Queue.AddOrUpdate(
                    dbType,
                    (_) => [],
                    (_, prev) => [.. prev]
                );
        }
    }

    /// <inheritdoc/>
    public void Push(IEntity Record) {
        if (Factories.ContainsKey(Record.Database)) {
            Queue.AddOrUpdate(
                    Record.Database,
                    (_) => [Record],
                    (_, prev) => [.. prev, Record]
                );

        } else {
            throw new Exception($"Tried to push a record for Disposition with no subscribed database owning factory ({Record.Database.Name}).");
        }
    }

    /// <inheritdoc/>
    public void Push(IEntity[] Records) {
        foreach (IEntity Record in Records) {
            Push(Record);
        }
    }

    /// <inheritdoc/>
    public void Dispose() {
        foreach (KeyValuePair<Type, IEntity[]> Database in Queue) {
            Type dbType = Database.Key;
            DatabaseFactory factory = Factories[dbType];

            using DbContext database = factory();
            IEnumerable<IEntity> committedEntities = Database.Value.Where(i => i.Id > 0).Reverse();

            foreach (IEntity committedEntity in committedEntities) {
                EntityEntry entry = database.Entry(committedEntity);
                if (entry.GetDatabaseValues() is null) {
                    continue;
                }

                // Delete ICollection Entities before deleting the main entity.
                foreach (var property in committedEntity.GetType().GetProperties()) {
                    if (typeof(IEnumerable<IEntity>).IsAssignableFrom(property.PropertyType)) {
                        if (property.GetValue(committedEntity) is IEnumerable<IEntity> collection) {
                            foreach (var item in collection) {
                                EntityEntry subEntry = database.Entry(item);
                                if (subEntry.GetDatabaseValues() is null) {
                                    continue;
                                }

                                subEntry.State = EntityState.Deleted;
                            }
                        }
                    }
                }


                entry.DetectChanges();
                entry.State = EntityState.Deleted;
                database.SaveChanges();
            }
        }
    }
}

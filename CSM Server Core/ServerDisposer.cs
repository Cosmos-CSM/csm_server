using System.Collections.Concurrent;

using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Extensions;
using CSM_Foundation_Core.Core.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace CSM_Server_Core;

/// <inheritdoc cref="CSM_Foundation_Core.Abstractions.Interfaces.IDisposer{IEntity}"/>
public class ServerDisposer
    : CSM_Foundation_Core.Abstractions.Interfaces.IDisposer<IEntity> {

    /// <summary>
    ///     Wheter the manager must keep track data or not.
    /// </summary>
    bool _isActive = false;

    /// <summary>
    ///     Service Provider to get database factories for instances location and handle data disposition on their corresponding contexts.
    /// </summary>
    readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Manager main disposition context, handles the current managed databases context types and group them to handle easier a batch
    ///     entities disposition based on their context type.
    /// </summary>
    readonly ConcurrentDictionary<Type, List<IEntity>> _dispositionStack = new();

    /// <summary>
    ///     Creates a new <see cref="ServerDisposer"/> instance.
    /// </summary>
    /// <param name="serviceProvider">
    ///     Service Provider locator, used to locate the database contexts factories.
    /// </param>
    public ServerDisposer(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public void Push(IEntity entity) {
        if (!_isActive) {
            return;
        }

        _dispositionStack.AddOrUpdate(
                entity.Database.GetType(),
                [entity],
                (Type _, List<IEntity> previousList) => {
                    lock (previousList) {
                        previousList.Add(entity);
                    }

                    return previousList;
                }
            );
    }

    public void Push(IEntity[] entities) {
        if (!_isActive) {
            return;
        }

        IEnumerable<IGrouping<Type, IEntity>> groupedEntities = entities.GroupBy(
                entity => entity.Database.GetType()
            );

        foreach (IGrouping<Type, IEntity> databaseEntitiesGroup in groupedEntities) {

            _dispositionStack.AddOrUpdate(
                    databaseEntitiesGroup.Key,
                    [.. databaseEntitiesGroup],
                    (Type _, List<IEntity> previousList) => {
                        lock (previousList) {
                            previousList.AddRange(databaseEntitiesGroup);
                        }

                        return previousList;
                    }
                );
        }
    }

    /// <summary>
    ///     Changes the manager state.
    /// </summary>
    /// <param name="active">
    ///     Wheter the manager must be tracking data or not.
    /// </param>
    public void ChangeState(bool active) {
        _isActive = active;
    }

    public void Dispose() {
        if (_dispositionStack.Empty()) {
            ConsoleUtils.Announce($"No records to dispose");
        }
        foreach (KeyValuePair<Type, List<IEntity>> disposeLine in _dispositionStack) {

            using IServiceScope servicerScope = _serviceProvider.CreateScope();

            DbContext Database = (DbContext)servicerScope.ServiceProvider.GetRequiredService(disposeLine.Key);

            ConsoleUtils.Announce($"Disposing db ({Database.GetType()})");
            if (disposeLine.Value is null || disposeLine.Value.Count == 0) {
                ConsoleUtils.Announce($"No records to dispose");
                continue;
            }
            int corrects = 0;
            int incorrects = 0;
            foreach (IEntity record in disposeLine.Value) {
                try {
                    Database.Remove(record);
                    Database.SaveChanges();

                    corrects++;
                    ConsoleUtils.Success($"Disposed: ({record.GetType()}) | ({record.Id})");
                } catch (DbUpdateConcurrencyException ex) {
                    foreach (EntityEntry entry in ex.Entries) {
                        if (entry.Entity.GetType() == record.GetType()) {
                            entry.State = EntityState.Detached;
                        }
                    }

                } catch (Exception ex) {
                    incorrects++;
                    ConsoleUtils.Warning($"No disposed: ({record.GetType()}) | ({record.Id}) |> ({ex.Message})");
                }
            }


            if (incorrects > 0) {
                ConsoleUtils.Warning($"Disposed with errors: (Errors: ({incorrects}) Successes: {corrects})");
            } else {
                ConsoleUtils.Success($"Disposed: ({corrects} elements) at ({Database.GetType()})");
            }
        }
        _dispositionStack.Clear();
    }
}

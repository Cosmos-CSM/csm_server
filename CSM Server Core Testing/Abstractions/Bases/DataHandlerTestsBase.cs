using System.Reflection;

using CSM_Database_Core.Core.Utils;
using CSM_Database_Core.Entities.Abstractions.Bases;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Utils;

using CSM_Server_Core_Testing.Disposition;
using CSM_Server_Core_Testing.Disposition.Abstractions.Bases;

using Microsoft.EntityFrameworkCore;

namespace CSM_Server_Core_Testing.Abstractions.Bases;

/// <summary>
///     Public Delegate for [Entity] factory [Quality] purposes.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the [Entity] to build.
/// </typeparam>
/// <param name="entropy">
///     Random 16 length <see cref="string"/> to generate unique properties records.
/// </param>
/// <returns>
///     The Entity stored in the database.
/// </returns>
public delegate TEntity EntityFactory<TEntity>(string entropy)
    where TEntity : class, IEntity;

/// <summary>
///     Represents a data handler tests base.
/// </summary>
public class DataHandlerTestsBase
    : IDisposable {

    /// <summary>
    ///     Quality disposition data manager, used to store to-remove entries after tests finished.
    /// </summary>
    protected readonly TestDataDisposer Disposer;

    /// <summary>
    ///     Database factories available for Samples Storing/Disposing.
    /// </summary>
    protected readonly Dictionary<Type, DatabaseFactory> Factories = [];

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="factories">
    ///     Collection of databases factories available for the handler to operate data.
    /// </param>
    public DataHandlerTestsBase(params DatabaseFactory[] factories) {
        foreach (DatabaseFactory factory in factories) {
            using DbContext dbContext = factory();
            Type dbType = dbContext.GetType();

            Factories.Add(dbType, factory);
        }

        Disposer = new TestDataDisposer(factories);
    }

    /// <inheritdoc/>
    public void Dispose() {
        Disposer.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Internal runner for <see cref="EntityFactory{TEntity}"/> utilizations, automatically sends the [Entropy] parameter. 
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] build by the <paramref name="factory"/>.
    /// </typeparam>
    /// <param name="factory">
    ///     [Entity] factory function.
    /// </param>
    /// <returns>
    ///     The generated [Entity] object.
    /// </returns>
    protected static TEntity2 RunEntityFactory<TEntity2>(EntityFactory<TEntity2> factory)
        where TEntity2 : class, IEntity {

        return factory(RandomUtils.String(16));
    }


    #region Storing

    /// <summary>
    ///     Stores the given <paramref name="entity"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] to store.
    /// </typeparam>
    /// <param name="entity">
    ///     [Entity] object instance properties to store into the database.
    /// </param>
    /// <returns>
    ///     The stored and updated [Entity] object values. 
    /// </returns>
    protected TEntity2 Store<TEntity2>(TEntity2 entity)
        where TEntity2 : class, IEntity {

        DbContext database = GetDatabase(entity.Database);

        entity = DatabaseUtils.SanitizeEntity(database, entity);
        database.Set<TEntity2>().Add(entity);
        database.SaveChanges();

        Disposer.Push(entity);

        return entity;
    }

    /// <summary>
    ///     Stores the given <paramref name="entities"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] to store.
    /// </typeparam>
    /// <param name="entities">
    ///     Entity collection to store as testing data.
    /// </param>
    /// <returns>
    ///     Stored entities data.
    /// </returns>
    protected async Task<TEntity2[]> Store<TEntity2>(TEntity2[] entities)
        where TEntity2 : class, IEntity {

        using DbContext database = GetDatabase(entities.First().Database);
        List<TEntity2> toSaveEntities = [];

        for (int i = 0; i < entities.Length; i++) {

            TEntity2 entity = entities[i];
            entity = DatabaseUtils.SanitizeEntity(database, entity);
            toSaveEntities.Add(entity);
        }

        await database.Set<TEntity2>().AddRangeAsync(toSaveEntities);
        await database.SaveChangesAsync();
        Disposer.Push([.. toSaveEntities]);

        return [.. toSaveEntities];
    }

    /// <summary>
    ///     Stores the [Entity] resulted by the <paramref name="entityFactory"/>.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] to store.
    /// </typeparam>
    /// <param name="entityFactory">
    ///     Factory to build the [Entity] to store.
    /// </param>
    /// <returns>
    ///     The stored and updated [Entity] object. 
    /// </returns>
    protected TEntity2 Store<TEntity2>(EntityFactory<TEntity2> entityFactory)
        where TEntity2 : class, IEntity {

        TEntity2 toStore = RunEntityFactory(entityFactory);
        toStore = Store(toStore);

        return toStore;
    }

    /// <summary>
    ///     Iterates based on <paramref name="quantity"/> to generate [Entities] to store based on <paramref name="entityFactory"/>.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] to store.
    /// </typeparam>
    /// <param name="quantity">
    ///     Quantity of iterations to call <paramref name="entityFactory"/> and store the factory result.
    /// </param>
    /// <param name="entityFactory">
    ///     Factory to build the [Entity] to store.
    /// </param>
    /// <returns>
    ///     The stored and updated [Entities] stored.
    /// </returns>
    protected async Task<TEntity2[]> Store<TEntity2>(int quantity, EntityFactory<TEntity2> entityFactory)
        where TEntity2 : class, IEntity {

        List<TEntity2> entities = [];

        using DbContext database = GetDatabase(RunEntityFactory(entityFactory).Database);
        for (int i = 0; i < quantity; i++) {

            TEntity2 entity = RunEntityFactory(entityFactory);
            entity = DatabaseUtils.SanitizeEntity(database, entity);
            entities.Add(entity);
        }

        await database.Set<TEntity2>().AddRangeAsync(entities);
        await database.SaveChangesAsync();
        Disposer.Push([.. entities]);

        return [.. entities];
    }

    /// <summary>
    /// Stores the specified common entity and its nested entities in the database.
    /// </summary>
    /// <remarks>This method processes the specified entity and its nested entities, adding them to the
    /// database. The method ensures that nested entities are stored in the correct order to maintain referential integrity.</remarks>
    /// <param name="common">The root entity to be stored. Nested entities within this entity will also be processed and stored.</param>
    /// <param name="save">A boolean value indicating whether to immediately save changes to the database. <see langword="true"/> to save
    /// changes after storing the entities; otherwise, <see langword="false"/>.</param>
    /// <returns>The root entity that was processed and stored.</returns>
    protected async Task<TCommon> Store<TCommon, TInternal, TExternal>(TCommon common, bool save = false) where TCommon : PartnerBridgeEntityBase<TInternal, TExternal>, new()
    where TInternal : class, IPartnerScopeEntity<TCommon>
    where TExternal : class, IPartnerScopeEntity<TCommon> {
        bool rootchecked = false;
        HashSet<IEntity> entitiesToAdd = [];
        using DbContext database = GetDatabase(new TCommon().Database);

        StoreNestedEntities<TCommon, TInternal, TExternal>(common, common, entitiesToAdd, rootchecked);

        foreach (IEntity entity in entitiesToAdd.Reverse()) {
            if (entity.Id == 0) {
                database.Add(entity);
                Disposer?.Push(entity);
            }
        }

        if (save) await database.SaveChangesAsync();

        return common;
    }

    /// <summary>
    /// Recurses through the nested entities of a common entity and stores them in a hash set to avoid duplicates.
    /// </summary>
    /// <param name="commonRoot"></param>
    /// <param name="entity">Current entity to process and store.</param>
    /// <param name="entitiesHash">List of stored entities. The content is verified to avoid duplications. </param>
    /// <param name="rootChecked">Flag for first recursive run.</param>
    private void StoreNestedEntities<TCommon, TInternal, TExternal>(TCommon commonRoot, IEntity entity, HashSet<IEntity> entitiesHash, bool rootChecked) where TCommon : PartnerBridgeEntityBase<TInternal, TExternal>
    where TInternal : class, IPartnerScopeEntity<TCommon>
    where TExternal : class, IPartnerScopeEntity<TCommon> {

        if (entity == null || entitiesHash.Contains(entity)) return;

        if (!rootChecked) {
            rootChecked = true;
            if (commonRoot.Internal != null) StoreNestedEntities<TCommon, TInternal, TExternal>(commonRoot, commonRoot.Internal, entitiesHash, rootChecked);
            if (commonRoot.External != null) StoreNestedEntities<TCommon, TInternal, TExternal>(commonRoot, commonRoot.External, entitiesHash, rootChecked);
            entitiesHash.Add(commonRoot);

        } else {
            entitiesHash.Add(entity);
        }

        Type type = entity.GetType();
        foreach (PropertyInfo prop in type.GetProperties()) {
            var value = prop.GetValue(entity);

            if (value is IEntity nestedEntity) {
                StoreNestedEntities<TCommon, TInternal, TExternal>(commonRoot, nestedEntity, entitiesHash, rootChecked);
            } else if (value is IEnumerable<IEntity> collection) {
                foreach (var item in collection) {
                    StoreNestedEntities<TCommon, TInternal, TExternal>(commonRoot, item, entitiesHash, rootChecked);
                }
            }
        }

    }

    #endregion

    /// <summary>
    ///    Retrieves the database instance for the given <paramref name="databaseType"/> based on the subscribed DatabaseFactories.
    /// </summary>
    /// <param name="databaseType">
    ///     <see cref="Type"/> of the database requested.
    /// </param>
    /// <returns>
    ///     The matched <see cref="Type"/> database context instance.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown when the requested database <see cref="Type"/> isn't found in the subcribed database factories.
    /// </exception>
    DbContext GetDatabase(Type databaseType) {
        return !Factories.TryGetValue(databaseType, out DatabaseFactory? factory)
            ? throw new Exception($"No factory subscribed for [({databaseType.Name})]")
            : factory();
    }
}

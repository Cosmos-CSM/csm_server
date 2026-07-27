using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Database_Testing.Managers;

using CSM_Server_Testing.Disposition;
using CSM_Server_Testing.Disposition.Abstractions.Bases;

using Microsoft.EntityFrameworkCore;

namespace CSM_Server_Testing.Abstractions.Bases;

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
    ///     Test data disposition manager, used to store to-remove entries after tests finished.
    /// </summary>
    protected readonly TestDataDisposer _disposer;

    /// <summary>
    ///      Base database storing manager having context of databases being used.
    /// </summary>
    protected readonly TestingStoreManager _testingStoreManager;

    /// <summary>
    ///     Database databaseFactories available for Samples Storing/Disposing.
    /// </summary>
    protected readonly Dictionary<Type, DatabaseFactory> _databasesFactories = [];

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="databaseFactories">
    ///     Collection of databases databaseFactories available for the handler to operate data.
    /// </param>
    public DataHandlerTestsBase(params DatabaseFactory[] databaseFactories) {
        foreach (DatabaseFactory factory in databaseFactories) {
            using DbContext dbContext = factory();
            Type dbType = dbContext.GetType();

            _databasesFactories.Add(dbType, factory);
        }

        _disposer = new TestDataDisposer(databaseFactories);
        _testingStoreManager = new TestingStoreManager(
                _disposer,
                [
                        ..databaseFactories.Select<DatabaseFactory, CSM_Database_Testing.Disposing.Abstractions.Bases.DatabaseFactory>(
                                f => () => f()
                            )
                    ]
            );
    }

    /// <inheritdoc/>
    public void Dispose() {
        _testingStoreManager.Dispose();
        GC.SuppressFinalize(this);
    }
}

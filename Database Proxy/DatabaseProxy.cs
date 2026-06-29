using CSM_Database_Core;

using Database_Proxy.Entities;

using Microsoft.EntityFrameworkCore;

namespace Database_Proxy;

/// <summary>
///     Represents a database proxy context for testing purposes.
/// </summary>
public class DatabaseProxy : DatabaseBase<DatabaseProxy> {

    /// <summary>
    ///     Customers Database Set.
    /// </summary>
    public DbSet<Customer> Customers { get; set; }  = default!;

    /// <summary>
    ///     Products Database Set.
    /// </summary>
    public DbSet<Product> Products { get; set; } = default!;

    /// <summary>
    ///     Orders Database Set.
    /// </summary>
    public DbSet<Order> Orders { get; set; } = default!;
}

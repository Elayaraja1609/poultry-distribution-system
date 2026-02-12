using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Supplier> Suppliers { get; }
    IRepository<Farm> Farms { get; }
    IRepository<Chicken> Chickens { get; }
    IRepository<Vehicle> Vehicles { get; }
    IRepository<Driver> Drivers { get; }
    IRepository<Cleaner> Cleaners { get; }
    IRepository<Shop> Shops { get; }
    IRepository<Purchase> Purchases { get; }
    IRepository<PurchaseItem> PurchaseItems { get; }
    IRepository<Distribution> Distributions { get; }
    IRepository<DistributionItem> DistributionItems { get; }
    IRepository<Delivery> Deliveries { get; }
    IRepository<Sale> Sales { get; }
    IRepository<Payment> Payments { get; }
    IRepository<FarmOperation> FarmOperations { get; }
    IRepository<StockMovement> StockMovements { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<Expense> Expenses { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Tenant> Tenants { get; }
    IRepository<User> Users { get; }
    IRepository<UserProfile> UserProfiles { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<UserRole> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

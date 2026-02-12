using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;
using PoultryDistributionSystem.Infrastructure.Data;
using PoultryDistributionSystem.Infrastructure.Repositories;

namespace PoultryDistributionSystem.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for transaction management
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repository properties
    private IRepository<Supplier>? _suppliers;
    private IRepository<Farm>? _farms;
    private IRepository<Chicken>? _chickens;
    private IRepository<Vehicle>? _vehicles;
    private IRepository<Driver>? _drivers;
    private IRepository<Cleaner>? _cleaners;
    private IRepository<Shop>? _shops;
    private IRepository<Purchase>? _purchases;
    private IRepository<PurchaseItem>? _purchaseItems;
    private IRepository<Distribution>? _distributions;
    private IRepository<DistributionItem>? _distributionItems;
    private IRepository<Delivery>? _deliveries;
    private IRepository<Sale>? _sales;
    private IRepository<Payment>? _payments;
    private IRepository<FarmOperation>? _farmOperations;
    private IRepository<StockMovement>? _stockMovements;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<Notification>? _notifications;
    private IRepository<Expense>? _expenses;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<Tenant>? _tenants;
    private IRepository<User>? _users;
    private IRepository<UserProfile>? _userProfiles;
    private IRepository<Role>? _roles;
    private IRepository<Permission>? _permissions;
    private IRepository<RolePermission>? _rolePermissions;
    private IRepository<UserRole>? _userRoles;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
    public IRepository<Farm> Farms => _farms ??= new Repository<Farm>(_context);
    public IRepository<Chicken> Chickens => _chickens ??= new Repository<Chicken>(_context);
    public IRepository<Vehicle> Vehicles => _vehicles ??= new Repository<Vehicle>(_context);
    public IRepository<Driver> Drivers => _drivers ??= new Repository<Driver>(_context);
    public IRepository<Cleaner> Cleaners => _cleaners ??= new Repository<Cleaner>(_context);
    public IRepository<Shop> Shops => _shops ??= new Repository<Shop>(_context);
    public IRepository<Purchase> Purchases => _purchases ??= new Repository<Purchase>(_context);
    public IRepository<PurchaseItem> PurchaseItems => _purchaseItems ??= new Repository<PurchaseItem>(_context);
    public IRepository<Distribution> Distributions => _distributions ??= new Repository<Distribution>(_context);
    public IRepository<DistributionItem> DistributionItems => _distributionItems ??= new Repository<DistributionItem>(_context);
    public IRepository<Delivery> Deliveries => _deliveries ??= new Repository<Delivery>(_context);
    public IRepository<Sale> Sales => _sales ??= new Repository<Sale>(_context);
    public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);
    public IRepository<FarmOperation> FarmOperations => _farmOperations ??= new Repository<FarmOperation>(_context);
    public IRepository<StockMovement> StockMovements => _stockMovements ??= new Repository<StockMovement>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);
    public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);
    public IRepository<Expense> Expenses => _expenses ??= new Repository<Expense>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);
    public IRepository<Tenant> Tenants => _tenants ??= new Repository<Tenant>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<UserProfile> UserProfiles => _userProfiles ??= new Repository<UserProfile>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<Permission> Permissions => _permissions ??= new Repository<Permission>(_context);
    public IRepository<RolePermission> RolePermissions => _rolePermissions ??= new Repository<RolePermission>(_context);
    public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

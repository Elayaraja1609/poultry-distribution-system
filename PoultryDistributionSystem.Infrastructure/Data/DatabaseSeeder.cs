using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Infrastructure.Data;

/// <summary>
/// Database seeder for initial and sample data.
/// Uses deterministic Guids for seed entities so relationships can be built.
/// </summary>
public static class DatabaseSeeder
{
    private const string DefaultPassword = "Test@123";
    private static readonly DateTime SeedTime = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database exists (no-op if using migrations and DB already created)
        await context.Database.EnsureCreatedAsync();

        await SeedRolesAsync(context);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context);
        await SeedTenantAsync(context);
        await SeedAdminUserAsync(context);
        await SeedSampleUsersAsync(context);
        await SeedSuppliersAsync(context);
        await SeedFarmsAsync(context);
        await SeedDriversAndCleanersAsync(context);
        await SeedVehiclesAsync(context);
        await SeedShopsAsync(context);
        await SeedChickensAsync(context);
        await SeedStockMovementsAsync(context);
        await SeedOrdersAsync(context);
        await SeedDistributionsAndDeliveriesAsync(context);
        await SeedSalesAndPaymentsAsync(context);
        await SeedExpensesAsync(context);
        await SeedNotificationsAsync(context);

        await context.SaveChangesAsync();
    }

    private static Guid Id(string seed)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(bytes);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new[]
        {
            new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System Administrator with full access", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = Guid.NewGuid(), Name = "ShopOwner", Description = "Shop owner who can place orders and manage their shop", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = Guid.NewGuid(), Name = "FarmManager", Description = "Farm manager who manages farms and chickens", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = Guid.NewGuid(), Name = "Driver", Description = "Driver who handles distributions", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = Guid.NewGuid(), Name = "Cleaner", Description = "Cleaner who handles loading/unloading", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await context.Roles.AddRangeAsync(roles);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;

        var permissions = new List<Permission>();
        var resources = new[] { "suppliers", "farms", "chickens", "vehicles", "distributions", "deliveries", "sales", "payments", "farm_operations", "reports", "users", "roles" };
        var actions = new[] { "create", "read", "update", "delete" };

        foreach (var resource in resources)
        {
            foreach (var action in actions)
            {
                permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = $"{resource}_{action}",
                    Resource = resource,
                    Action = action,
                    Description = $"Permission to {action} {resource}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.Permissions.AddRangeAsync(permissions);
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
            return;

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null) return;

        var allPermissions = await context.Permissions.ToListAsync();
        var rolePermissions = allPermissions.Select(p => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminRole.Id,
            PermissionId = p.Id,
            CreatedAt = DateTime.UtcNow
        });

        await context.RolePermissions.AddRangeAsync(rolePermissions);
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Username == "admin"))
            return;

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null) return;

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@poultrysystem.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = Domain.Enums.UserRole.Admin,
            IsActive = true,
            TenantId = Id("tenant-default"),
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(adminUser);

        // Create admin user profile
        var adminProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            FullName = "System Administrator",
            Phone = "+1234567890",
            CreatedAt = DateTime.UtcNow
        };

        await context.UserProfiles.AddAsync(adminProfile);

        // Assign admin role to user
        var userRoleEntity = new Domain.Entities.UserRole
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddAsync(userRoleEntity);
    }

    private static async Task SeedTenantAsync(ApplicationDbContext context)
    {
        if (await context.Tenants.AnyAsync())
            return;

        var tenant = new Tenant
        {
            Id = Id("tenant-default"),
            Name = "Default Tenant",
            Subdomain = "default",
            Domain = "poultry.local",
            IsActive = true,
            SubscriptionPlan = "Professional",
            MaxUsers = 50,
            MaxShops = 20,
            MaxFarms = 10,
            SubscriptionExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = SeedTime
        };
        await context.Tenants.AddAsync(tenant);
    }

    private static async Task SeedSampleUsersAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Username == "shopowner1"))
            return;

        var shopOwnerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ShopOwner");
        var farmManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "FarmManager");
        var driverRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Driver");
        if (shopOwnerRole == null || farmManagerRole == null || driverRole == null)
            return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);

        var shopOwner = new User
        {
            Id = Id("user-shopowner1"),
            Username = "shopowner1",
            Email = "shopowner1@poultrysystem.com",
            PasswordHash = passwordHash,
            Role = Domain.Enums.UserRole.ShopOwner,
            IsActive = true,
            TenantId = Id("tenant-default"),
            CreatedAt = SeedTime
        };
        await context.Users.AddAsync(shopOwner);
        await context.UserProfiles.AddAsync(new UserProfile { Id = Guid.NewGuid(), UserId = shopOwner.Id, FullName = "Jane Shop Owner", Phone = "+1987654321", CreatedAt = SeedTime });
        await context.UserRoles.AddAsync(new Domain.Entities.UserRole { Id = Guid.NewGuid(), UserId = shopOwner.Id, RoleId = shopOwnerRole.Id, AssignedAt = SeedTime, CreatedAt = SeedTime });

        var farmManager = new User
        {
            Id = Id("user-farmmanager1"),
            Username = "farmmanager1",
            Email = "farmmanager1@poultrysystem.com",
            PasswordHash = passwordHash,
            Role = Domain.Enums.UserRole.FarmManager,
            IsActive = true,
            TenantId = Id("tenant-default"),
            CreatedAt = SeedTime
        };
        await context.Users.AddAsync(farmManager);
        await context.UserProfiles.AddAsync(new UserProfile { Id = Guid.NewGuid(), UserId = farmManager.Id, FullName = "Bob Farm Manager", Phone = "+1122334455", CreatedAt = SeedTime });
        await context.UserRoles.AddAsync(new Domain.Entities.UserRole { Id = Guid.NewGuid(), UserId = farmManager.Id, RoleId = farmManagerRole.Id, AssignedAt = SeedTime, CreatedAt = SeedTime });

        var driver = new User
        {
            Id = Id("user-driver1"),
            Username = "driver1",
            Email = "driver1@poultrysystem.com",
            PasswordHash = passwordHash,
            Role = Domain.Enums.UserRole.Driver,
            IsActive = true,
            TenantId = Id("tenant-default"),
            CreatedAt = SeedTime
        };
        await context.Users.AddAsync(driver);
        await context.UserProfiles.AddAsync(new UserProfile { Id = Guid.NewGuid(), UserId = driver.Id, FullName = "Mike Driver", Phone = "+1555666777", CreatedAt = SeedTime });
        await context.UserRoles.AddAsync(new Domain.Entities.UserRole { Id = Guid.NewGuid(), UserId = driver.Id, RoleId = driverRole.Id, AssignedAt = SeedTime, CreatedAt = SeedTime });
    }

    private static async Task SeedSuppliersAsync(ApplicationDbContext context)
    {
        if (await context.Suppliers.AnyAsync())
            return;

        var suppliers = new[]
        {
            new Supplier { Id = Id("supplier-1"), Name = "Green Valley Poultry", ContactPerson = "John Smith", Phone = "+1111111111", Email = "john@greenvalley.com", Address = "123 Farm Rd, Valley", IsActive = true, CreatedAt = SeedTime },
            new Supplier { Id = Id("supplier-2"), Name = "Sunrise Farms Co", ContactPerson = "Mary Johnson", Phone = "+1222222222", Email = "mary@sunrisefarms.com", Address = "456 Sunrise Ave", IsActive = true, CreatedAt = SeedTime },
            new Supplier { Id = Id("supplier-3"), Name = "Country Fresh Chickens", ContactPerson = "Tom Brown", Phone = "+1333333333", Email = "tom@countryfresh.com", Address = "789 Country Lane", IsActive = true, CreatedAt = SeedTime }
        };
        await context.Suppliers.AddRangeAsync(suppliers);
    }

    private static async Task SeedFarmsAsync(ApplicationDbContext context)
    {
        if (await context.Farms.AnyAsync())
            return;

        var farms = new[]
        {
            new Farm { Id = Id("farm-1"), Name = "North Farm", Location = "North Region, Block A", Capacity = 5000, CurrentCount = 3200, ManagerName = "Bob Farm Manager", Phone = "+1122334455", IsActive = true, CreatedAt = SeedTime },
            new Farm { Id = Id("farm-2"), Name = "South Farm", Location = "South Region, Block B", Capacity = 4000, CurrentCount = 2800, ManagerName = "Alice Manager", Phone = "+1444555666", IsActive = true, CreatedAt = SeedTime }
        };
        await context.Farms.AddRangeAsync(farms);
    }

    private static async Task SeedDriversAndCleanersAsync(ApplicationDbContext context)
    {
        if (await context.Drivers.AnyAsync())
            return;

        var drivers = new[]
        {
            new Driver { Id = Id("driver-1"), Name = "Mike Driver", Phone = "+1555666777", LicenseNumber = "DL-001-2024", IsActive = true, CreatedAt = SeedTime },
            new Driver { Id = Id("driver-2"), Name = "Steve Wheeler", Phone = "+1666777888", LicenseNumber = "DL-002-2024", IsActive = true, CreatedAt = SeedTime }
        };
        await context.Drivers.AddRangeAsync(drivers);

        var cleaners = new[]
        {
            new Cleaner { Id = Id("cleaner-1"), Name = "Carl Cleaner", Phone = "+1777888999", IsActive = true, CreatedAt = SeedTime },
            new Cleaner { Id = Id("cleaner-2"), Name = "Dave Loader", Phone = "+1888999000", IsActive = true, CreatedAt = SeedTime }
        };
        await context.Cleaners.AddRangeAsync(cleaners);
    }

    private static async Task SeedVehiclesAsync(ApplicationDbContext context)
    {
        if (await context.Vehicles.AnyAsync())
            return;

        var vehicles = new[]
        {
            new Vehicle { Id = Id("vehicle-1"), VehicleNumber = "VH-001", Model = "Refrigerated Truck 5T", Capacity = 500, DriverId = Id("driver-1"), CleanerId = Id("cleaner-1"), IsActive = true, CreatedAt = SeedTime },
            new Vehicle { Id = Id("vehicle-2"), VehicleNumber = "VH-002", Model = "Van 2T", Capacity = 200, DriverId = Id("driver-2"), CleanerId = Id("cleaner-2"), IsActive = true, CreatedAt = SeedTime }
        };
        await context.Vehicles.AddRangeAsync(vehicles);
    }

    private static async Task SeedShopsAsync(ApplicationDbContext context)
    {
        if (await context.Shops.AnyAsync())
            return;

        var shops = new[]
        {
            new Shop { Id = Id("shop-1"), Name = "Downtown Poultry Shop", OwnerName = "Jane Shop Owner", Phone = "+1987654321", Email = "downtown@shop.com", Address = "100 Main St", IsActive = true, CreatedAt = SeedTime },
            new Shop { Id = Id("shop-2"), Name = "Market Fresh Store", OwnerName = "Peter Owner", Phone = "+1999888777", Email = "market@shop.com", Address = "200 Market Ave", IsActive = true, CreatedAt = SeedTime }
        };
        await context.Shops.AddRangeAsync(shops);
    }

    private static async Task SeedChickensAsync(ApplicationDbContext context)
    {
        if (await context.Chickens.AnyAsync())
            return;

        var purchaseDate = SeedTime.AddDays(-30);
        var chickens = new[]
        {
            new Chicken { Id = Id("chicken-b1"), BatchNumber = "BATCH-2025-001", SupplierId = Id("supplier-1"), FarmId = Id("farm-1"), PurchaseDate = purchaseDate, Quantity = 500, AgeDays = 35, WeightKg = 1.8m, Status = ChickenStatus.InFarm, HealthStatus = HealthStatus.Healthy, CreatedAt = SeedTime },
            new Chicken { Id = Id("chicken-b2"), BatchNumber = "BATCH-2025-002", SupplierId = Id("supplier-1"), FarmId = Id("farm-1"), PurchaseDate = purchaseDate, Quantity = 400, AgeDays = 28, WeightKg = 1.5m, Status = ChickenStatus.InFarm, HealthStatus = HealthStatus.Healthy, CreatedAt = SeedTime },
            new Chicken { Id = Id("chicken-b3"), BatchNumber = "BATCH-2025-003", SupplierId = Id("supplier-2"), FarmId = Id("farm-2"), PurchaseDate = purchaseDate.AddDays(-7), Quantity = 600, AgeDays = 42, WeightKg = 2.0m, Status = ChickenStatus.ReadyForDistribution, HealthStatus = HealthStatus.Healthy, CreatedAt = SeedTime }
        };
        await context.Chickens.AddRangeAsync(chickens);
    }

    private static async Task SeedStockMovementsAsync(ApplicationDbContext context)
    {
        if (await context.StockMovements.AnyAsync())
            return;

        var movements = new[]
        {
            new StockMovement { Id = Guid.NewGuid(), FarmId = Id("farm-1"), ChickenId = Id("chicken-b1"), MovementType = StockMovementType.In, Quantity = 500, PreviousQuantity = 0, NewQuantity = 500, Reason = "Initial receipt", MovementDate = SeedTime.AddDays(-30), CreatedAt = SeedTime },
            new StockMovement { Id = Guid.NewGuid(), FarmId = Id("farm-1"), ChickenId = Id("chicken-b2"), MovementType = StockMovementType.In, Quantity = 400, PreviousQuantity = 0, NewQuantity = 400, Reason = "Initial receipt", MovementDate = SeedTime.AddDays(-30), CreatedAt = SeedTime },
            new StockMovement { Id = Guid.NewGuid(), FarmId = Id("farm-2"), ChickenId = Id("chicken-b3"), MovementType = StockMovementType.In, Quantity = 600, PreviousQuantity = 0, NewQuantity = 600, Reason = "Initial receipt", MovementDate = SeedTime.AddDays(-37), CreatedAt = SeedTime }
        };
        await context.StockMovements.AddRangeAsync(movements);
    }

    private static async Task SeedOrdersAsync(ApplicationDbContext context)
    {
        if (await context.Orders.AnyAsync())
            return;

        var orderDate = SeedTime.AddDays(-5);
        var requestedDate = SeedTime.AddDays(2);
        var order = new Order
        {
            Id = Id("order-1"),
            ShopId = Id("shop-1"),
            OrderDate = orderDate,
            RequestedDeliveryDate = requestedDate,
            Status = OrderStatus.Approved,
            TotalAmount = 4500m,
            FulfillmentStatus = FulfillmentStatus.Partial,
            ApprovedBy = (await context.Users.FirstOrDefaultAsync(u => u.Username == "admin"))?.Id,
            ApprovedAt = orderDate.AddHours(2),
            CreatedAt = SeedTime
        };
        await context.Orders.AddAsync(order);
        await context.OrderItems.AddRangeAsync(
            new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, ChickenId = Id("chicken-b1"), RequestedQuantity = 100, FulfilledQuantity = 50, UnitPrice = 25m, TotalPrice = 2500m, CreatedAt = SeedTime },
            new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, ChickenId = Id("chicken-b3"), RequestedQuantity = 80, FulfilledQuantity = 80, UnitPrice = 25m, TotalPrice = 2000m, CreatedAt = SeedTime }
        );

        var order2 = new Order
        {
            Id = Id("order-2"),
            ShopId = Id("shop-2"),
            OrderDate = SeedTime.AddDays(-2),
            RequestedDeliveryDate = SeedTime.AddDays(3),
            Status = OrderStatus.Pending,
            TotalAmount = 3000m,
            FulfillmentStatus = FulfillmentStatus.None,
            CreatedAt = SeedTime
        };
        await context.Orders.AddAsync(order2);
        await context.OrderItems.AddAsync(new OrderItem { Id = Guid.NewGuid(), OrderId = order2.Id, ChickenId = Id("chicken-b2"), RequestedQuantity = 120, FulfilledQuantity = 0, UnitPrice = 25m, TotalPrice = 3000m, CreatedAt = SeedTime });
    }

    private static async Task SeedDistributionsAndDeliveriesAsync(ApplicationDbContext context)
    {
        if (await context.Distributions.AnyAsync())
            return;

        var distDate = SeedTime.AddDays(-3);
        var dist = new Distribution
        {
            Id = Id("dist-1"),
            VehicleId = Id("vehicle-1"),
            ScheduledDate = distDate,
            Status = DistributionStatus.Completed,
            CreatedAt = SeedTime
        };
        await context.Distributions.AddAsync(dist);

        await context.DistributionItems.AddRangeAsync(
            new DistributionItem { Id = Guid.NewGuid(), DistributionId = dist.Id, ChickenId = Id("chicken-b1"), ShopId = Id("shop-1"), Quantity = 50, DeliveryStatus = DistributionItemStatus.Delivered, DeliveredAt = distDate.AddHours(2), CreatedAt = SeedTime },
            new DistributionItem { Id = Guid.NewGuid(), DistributionId = dist.Id, ChickenId = Id("chicken-b3"), ShopId = Id("shop-1"), Quantity = 80, DeliveryStatus = DistributionItemStatus.Delivered, DeliveredAt = distDate.AddHours(2), CreatedAt = SeedTime }
        );

        var delivery = new Delivery
        {
            Id = Id("delivery-1"),
            DistributionId = dist.Id,
            ShopId = Id("shop-1"),
            DeliveryDate = distDate,
            TotalQuantity = 130,
            VerifiedQuantity = 130,
            DeliveryStatus = DeliveryStatus.Completed,
            Notes = "All items received",
            CreatedAt = SeedTime
        };
        await context.Deliveries.AddAsync(delivery);
    }

    private static async Task SeedSalesAndPaymentsAsync(ApplicationDbContext context)
    {
        if (await context.Sales.AnyAsync())
            return;

        var deliveryId = Id("delivery-1");
        var saleDate = SeedTime.AddDays(-3);
        var sale = new Sale
        {
            Id = Id("sale-1"),
            DeliveryId = deliveryId,
            ShopId = Id("shop-1"),
            SaleDate = saleDate,
            TotalAmount = 3250m,
            PaymentStatus = PaymentStatus.Paid,
            PaymentMethod = PaymentMethod.BankTransfer,
            CreatedAt = SeedTime
        };
        await context.Sales.AddAsync(sale);

        await context.Payments.AddAsync(new Payment
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            Amount = 3250m,
            PaymentDate = saleDate,
            PaymentMethod = PaymentMethod.BankTransfer,
            Notes = "Full payment received",
            CreatedAt = SeedTime
        });
    }

    private static async Task SeedExpensesAsync(ApplicationDbContext context)
    {
        if (await context.Expenses.AnyAsync())
            return;

        var expenses = new[]
        {
            new Expense { Id = Guid.NewGuid(), ExpenseType = ExpenseType.Fuel, Category = "Fuel", Amount = 350m, Description = "Diesel for VH-001", ExpenseDate = SeedTime.AddDays(-10), VehicleId = Id("vehicle-1"), CreatedAt = SeedTime },
            new Expense { Id = Guid.NewGuid(), ExpenseType = ExpenseType.Salary, Category = "Salary", Amount = 2500m, Description = "Monthly driver salary", ExpenseDate = SeedTime.AddDays(-15), CreatedAt = SeedTime },
            new Expense { Id = Guid.NewGuid(), ExpenseType = ExpenseType.Feed, Category = "Feed", Amount = 1200m, Description = "Feed for North Farm", ExpenseDate = SeedTime.AddDays(-7), FarmId = Id("farm-1"), CreatedAt = SeedTime }
        };
        await context.Expenses.AddRangeAsync(expenses);
    }

    private static async Task SeedNotificationsAsync(ApplicationDbContext context)
    {
        if (await context.Notifications.AnyAsync())
            return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (admin == null)
            return;

        var notifications = new[]
        {
            new Notification { Id = Guid.NewGuid(), UserId = admin.Id, Type = NotificationType.SystemAlert, Title = "Welcome", Message = "System seeded with sample data. You can log in as admin / Admin@123", IsRead = false, CreatedAt = SeedTime },
            new Notification { Id = Guid.NewGuid(), UserId = admin.Id, Type = NotificationType.OrderApproved, Title = "Order Approved", Message = "Order #1 has been approved.", IsRead = true, ReadAt = SeedTime.AddHours(1), RelatedEntityType = "Order", RelatedEntityId = Id("order-1"), CreatedAt = SeedTime }
        };
        await context.Notifications.AddRangeAsync(notifications);
    }
}

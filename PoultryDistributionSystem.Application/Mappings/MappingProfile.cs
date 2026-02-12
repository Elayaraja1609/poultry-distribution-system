using AutoMapper;
using PoultryDistributionSystem.Application.DTOs.Auth;
using PoultryDistributionSystem.Application.DTOs.Chicken;
using PoultryDistributionSystem.Application.DTOs.Cleaner;
using PoultryDistributionSystem.Application.DTOs.Delivery;
using PoultryDistributionSystem.Application.DTOs.Distribution;
using PoultryDistributionSystem.Application.DTOs.Driver;
using PoultryDistributionSystem.Application.DTOs.Farm;
using PoultryDistributionSystem.Application.DTOs.Audit;
using PoultryDistributionSystem.Application.DTOs.Expense;
using PoultryDistributionSystem.Application.DTOs.Inventory;
using PoultryDistributionSystem.Application.DTOs.Notification;
using PoultryDistributionSystem.Application.DTOs.Order;
using PoultryDistributionSystem.Application.DTOs.Payment;
using PoultryDistributionSystem.Application.DTOs.Sale;
using PoultryDistributionSystem.Application.DTOs.Tenant;
using PoultryDistributionSystem.Application.DTOs.Shop;
using PoultryDistributionSystem.Application.DTOs.Supplier;
using PoultryDistributionSystem.Application.DTOs.Vehicle;
using PoultryDistributionSystem.Domain.Entities;

namespace PoultryDistributionSystem.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.FullName : null));

        // Supplier mappings
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Purchases, opt => opt.Ignore())
            .ForMember(dest => dest.Chickens, opt => opt.Ignore());
        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Purchases, opt => opt.Ignore())
            .ForMember(dest => dest.Chickens, opt => opt.Ignore());

        // Farm mappings
        CreateMap<Farm, FarmDto>();
        CreateMap<CreateFarmDto, Farm>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Chickens, opt => opt.Ignore())
            .ForMember(dest => dest.FarmOperations, opt => opt.Ignore());
        CreateMap<UpdateFarmDto, Farm>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentCount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Chickens, opt => opt.Ignore())
            .ForMember(dest => dest.FarmOperations, opt => opt.Ignore());

        // Chicken mappings
        CreateMap<Chicken, ChickenDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.Ignore())
            .ForMember(dest => dest.FarmName, opt => opt.Ignore());
        CreateMap<CreateChickenDto, Chicken>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AgeDays, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.HealthStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseItems, opt => opt.Ignore())
            .ForMember(dest => dest.DistributionItems, opt => opt.Ignore())
            .ForMember(dest => dest.FarmOperations, opt => opt.Ignore());

        // Vehicle mappings
        CreateMap<Vehicle, VehicleDto>()
            .ForMember(dest => dest.DriverName, opt => opt.Ignore())
            .ForMember(dest => dest.CleanerName, opt => opt.Ignore());
        CreateMap<CreateVehicleDto, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.Cleaner, opt => opt.Ignore())
            .ForMember(dest => dest.Distributions, opt => opt.Ignore());
        CreateMap<UpdateVehicleDto, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.Cleaner, opt => opt.Ignore())
            .ForMember(dest => dest.Distributions, opt => opt.Ignore());

        // Driver mappings
        CreateMap<Driver, DriverDto>();
        CreateMap<CreateDriverDto, Driver>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

        // Cleaner mappings
        CreateMap<Cleaner, CleanerDto>();
        CreateMap<CreateCleanerDto, Cleaner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

        // Distribution mappings
        CreateMap<Distribution, DistributionDto>()
            .ForMember(dest => dest.VehicleNumber, opt => opt.Ignore())
            .ForMember(dest => dest.DriverName, opt => opt.Ignore())
            .ForMember(dest => dest.CleanerName, opt => opt.Ignore())
            .ForMember(dest => dest.TotalItems, opt => opt.Ignore());

        // Delivery mappings
        CreateMap<Delivery, DeliveryDto>()
            .ForMember(dest => dest.ShopName, opt => opt.Ignore());

        // Sale mappings
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.ShopName, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAmount, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingAmount, opt => opt.Ignore());
        CreateMap<CreateSaleDto, Sale>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentGatewayTransactionId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Delivery, opt => opt.Ignore())
            .ForMember(dest => dest.Shop, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore());

        // Payment mappings
        CreateMap<Payment, PaymentDto>();
        CreateMap<CreatePaymentDto, Payment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Sale, opt => opt.Ignore());

        // Shop mappings
        CreateMap<Shop, ShopDto>();
        CreateMap<CreateShopDto, Shop>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DistributionItems, opt => opt.Ignore())
            .ForMember(dest => dest.Deliveries, opt => opt.Ignore())
            .ForMember(dest => dest.Sales, opt => opt.Ignore());

        // Inventory mappings
        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(dest => dest.FarmName, opt => opt.Ignore())
            .ForMember(dest => dest.BatchNumber, opt => opt.Ignore());

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.ShopName, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore());
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.BatchNumber, opt => opt.Ignore());

        // Notification mappings
        CreateMap<Notification, NotificationDto>();

        // Expense mappings
        CreateMap<Expense, ExpenseDto>()
            .ForMember(dest => dest.VehicleNumber, opt => opt.Ignore())
            .ForMember(dest => dest.FarmName, opt => opt.Ignore());
        CreateMap<CreateExpenseDto, Expense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore());

        // Audit mappings
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore());

        // Tenant mappings
        CreateMap<Tenant, TenantDto>();
        CreateMap<CreateTenantDto, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.MaxUsers, opt => opt.Ignore())
            .ForMember(dest => dest.MaxShops, opt => opt.Ignore())
            .ForMember(dest => dest.MaxFarms, opt => opt.Ignore())
            .ForMember(dest => dest.Settings, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore());
    }
}

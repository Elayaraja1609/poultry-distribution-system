using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// Background service for sending scheduled notifications
/// </summary>
public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<Domain.Interfaces.IUnitOfWork>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Check for scheduled deliveries tomorrow
                var tomorrow = DateTime.UtcNow.AddDays(1).Date;
                var distributions = await unitOfWork.Distributions.FindAsync(
                    d => d.ScheduledDate.Date == tomorrow && 
                         d.Status == Domain.Enums.DistributionStatus.Scheduled &&
                         !d.IsDeleted,
                    stoppingToken);

                foreach (var distribution in distributions)
                {
                    var distributionItems = await unitOfWork.DistributionItems.FindAsync(
                        di => di.DistributionId == distribution.Id,
                        stoppingToken);

                    var shopIds = distributionItems.Select(di => di.ShopId).Distinct();
                    foreach (var shopId in shopIds)
                    {
                        var shop = await unitOfWork.Shops.GetByIdAsync(shopId, stoppingToken);
                        if (shop != null)
                        {
                            var shopUser = await unitOfWork.Users.FirstOrDefaultAsync(
                                u => u.Email == shop.Email && !u.IsDeleted,
                                stoppingToken);
                            if (shopUser != null)
                            {
                                await notificationService.SendDeliveryScheduledNotificationAsync(
                                    distribution.Id,
                                    shopUser.Id,
                                    stoppingToken);
                            }
                        }
                    }
                }

                // Check for pending payments (daily reminder)
                var sales = await unitOfWork.Sales.FindAsync(
                    s => (s.PaymentStatus == Domain.Enums.PaymentStatus.Pending ||
                          s.PaymentStatus == Domain.Enums.PaymentStatus.Partial) &&
                         !s.IsDeleted,
                    stoppingToken);

                foreach (var sale in sales)
                {
                    var shop = await unitOfWork.Shops.GetByIdAsync(sale.ShopId, stoppingToken);
                    if (shop != null)
                    {
                        var shopUsers = await unitOfWork.Users.FindAsync(
                            u => u.Email == shop.Email && !u.IsDeleted,
                            stoppingToken);
                        var shopUser = shopUsers.FirstOrDefault();
                        if (shopUser != null)
                        {
                            // Only send reminder if last payment was more than 7 days ago
                            var payments = await unitOfWork.Payments.FindAsync(
                                p => p.SaleId == sale.Id,
                                stoppingToken);
                            var lastPayment = payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
                            
                            if (lastPayment == null || (DateTime.UtcNow - lastPayment.PaymentDate).TotalDays >= 7)
                            {
                                await notificationService.SendPaymentReminderNotificationAsync(
                                    sale.Id,
                                    shopUser.Id,
                                    stoppingToken);
                            }
                        }
                    }
                }

                // Check for low stock alerts
                var farms = await unitOfWork.Farms.FindAsync(f => !f.IsDeleted, stoppingToken);
                foreach (var farm in farms)
                {
                    var stockPercentage = farm.Capacity > 0 ? (farm.CurrentCount / (double)farm.Capacity) * 100 : 0;
                    if (stockPercentage < 20) // Low stock threshold
                    {
                        var adminUsers = await unitOfWork.Users.FindAsync(
                            u => u.Role == Domain.Enums.UserRole.Admin && !u.IsDeleted,
                            stoppingToken);

                        foreach (var admin in adminUsers)
                        {
                            try
                            {
                                await notificationService.CreateNotificationAsync(
                                    new Application.DTOs.Notification.CreateNotificationDto
                                    {
                                        UserId = admin.Id,
                                        Type = NotificationType.LowStock,
                                        Title = "Low Stock Alert",
                                        Message = $"Farm {farm.Name} has low stock ({farm.CurrentCount}/{farm.Capacity}). Stock level: {stockPercentage:F1}%",
                                        RelatedEntityType = "Farm",
                                        RelatedEntityId = farm.Id
                                    },
                                    stoppingToken);
                            }
                            catch
                            {
                                // Log error but continue
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification background service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}

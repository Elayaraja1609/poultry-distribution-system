using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Audit;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Audit service interface
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(string entityType, Guid entityId, AuditAction action, Guid? userId, string? oldValues, string? newValues, string? ipAddress, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? entityType, Guid? userId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

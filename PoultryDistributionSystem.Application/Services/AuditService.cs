using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Audit;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Enums;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Audit service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task LogActionAsync(string entityType, Guid entityId, AuditAction action, Guid? userId, string? oldValues, string? newValues, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? entityType, Guid? userId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allLogs = await _unitOfWork.AuditLogs.FindAsync(a => !a.IsDeleted, cancellationToken);

        if (!string.IsNullOrEmpty(entityType))
        {
            allLogs = allLogs.Where(a => a.EntityType == entityType);
        }

        if (userId.HasValue)
        {
            allLogs = allLogs.Where(a => a.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            allLogs = allLogs.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            allLogs = allLogs.Where(a => a.Timestamp <= endDate.Value);
        }

        var logsList = allLogs.OrderByDescending(a => a.Timestamp).ToList();
        var totalCount = logsList.Count;
        var pagedLogs = logsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<AuditLogDto>();
        foreach (var log in pagedLogs)
        {
            var dto = _mapper.Map<AuditLogDto>(log);
            if (log.UserId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(log.UserId.Value, cancellationToken);
                if (user != null)
                {
                    var profile = user.UserProfile;
                    dto.UserName = profile?.FullName ?? user.Username ?? "Unknown";
                }
            }
            items.Add(dto);
        }

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

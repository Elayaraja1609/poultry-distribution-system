namespace PoultryDistributionSystem.Infrastructure.Services.Interfaces;

/// <summary>
/// PDF service interface for generating invoices and receipts
/// </summary>
public interface IPdfService
{
    Task<byte[]> GenerateInvoiceAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateReceiptAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateExpenseReceiptAsync(Guid expenseId, CancellationToken cancellationToken = default);
}

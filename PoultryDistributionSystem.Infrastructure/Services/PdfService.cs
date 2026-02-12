using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// PDF service implementation using QuestPDF
/// </summary>
public class PdfService : IPdfService
{
    private readonly IUnitOfWork _unitOfWork;

    public PdfService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoiceAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");
        }

        var shop = await _unitOfWork.Shops.GetByIdAsync(sale.ShopId, cancellationToken);
        var delivery = await _unitOfWork.Deliveries.GetByIdAsync(sale.DeliveryId, cancellationToken);
        
        // Get distribution items from the delivery's distribution and prepare data
        var invoiceItems = new List<(string Description, int Quantity, decimal Amount)>();
        if (delivery != null)
        {
            var distribution = await _unitOfWork.Distributions.GetByIdAsync(delivery.DistributionId, cancellationToken);
            if (distribution != null)
            {
                var items = await _unitOfWork.DistributionItems.FindAsync(
                    di => di.DistributionId == distribution.Id && di.ShopId == sale.ShopId,
                    cancellationToken);
                var itemsList = items.ToList();
                
                if (itemsList.Count > 0)
                {
                    var unitPrice = delivery.VerifiedQuantity > 0 ? sale.TotalAmount / delivery.VerifiedQuantity : 0;
                    foreach (var item in itemsList)
                    {
                        var chicken = await _unitOfWork.Chickens.GetByIdAsync(item.ChickenId, cancellationToken);
                        var itemTotal = item.Quantity * unitPrice;
                        invoiceItems.Add((chicken?.BatchNumber ?? "N/A", item.Quantity, itemTotal));
                    }
                }
            }
        }
        
        // Fallback if no items found
        if (invoiceItems.Count == 0)
        {
            invoiceItems.Add(("Poultry Delivery", delivery?.VerifiedQuantity ?? 0, sale.TotalAmount));
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text("INVOICE")
                    .SemiBold().FontSize(20);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        // Invoice details
                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Invoice #: {sale.Id.ToString().Substring(0, 8)}").SemiBold();
                                col.Item().Text($"Date: {sale.SaleDate:yyyy-MM-dd}");
                                col.Item().Text($"Status: {sale.PaymentStatus}");
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Bill To:").SemiBold();
                                col.Item().Text(shop?.Name ?? "N/A");
                                col.Item().Text(shop?.Address ?? "");
                                col.Item().Text(shop?.Phone ?? "");
                            });
                        });

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Items table
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Quantity").SemiBold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Amount").SemiBold();
                            });

                            foreach (var item in invoiceItems)
                            {
                                table.Cell().Element(CellStyle).Text(item.Description);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"${item.Amount:F2}");
                            }
                        });

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Totals
                        x.Item().AlignRight().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(100).Text("Subtotal:").SemiBold();
                                row.ConstantItem(100).AlignRight().Text($"${sale.TotalAmount:F2}");
                            });
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(100).Text("Total:").SemiBold().FontSize(12);
                                row.ConstantItem(100).AlignRight().Text($"${sale.TotalAmount:F2}").SemiBold().FontSize(12);
                            });
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Thank you for your business!");
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateReceiptAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment with ID {paymentId} not found");
        }

        var sale = await _unitOfWork.Sales.GetByIdAsync(payment.SaleId, cancellationToken);
        var shop = sale != null ? await _unitOfWork.Shops.GetByIdAsync(sale.ShopId, cancellationToken) : null;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text("PAYMENT RECEIPT")
                    .SemiBold().FontSize(20);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Receipt #: {payment.Id.ToString().Substring(0, 8)}").SemiBold();
                                col.Item().Text($"Date: {payment.PaymentDate:yyyy-MM-dd}");
                                col.Item().Text($"Payment Method: {payment.PaymentMethod}");
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Paid By:").SemiBold();
                                col.Item().Text(shop?.Name ?? "N/A");
                                col.Item().Text(shop?.Address ?? "");
                            });
                        });

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        x.Item().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(150).Text("Sale ID:").SemiBold();
                                row.RelativeItem().Text(sale?.Id.ToString().Substring(0, 8) ?? "N/A");
                            });
                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(150).Text("Amount Paid:").SemiBold();
                                row.RelativeItem().Text($"${payment.Amount:F2}").FontSize(14);
                            });
                            if (!string.IsNullOrEmpty(payment.Notes))
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantColumn(150).Text("Notes:").SemiBold();
                                    row.RelativeItem().Text(payment.Notes);
                                });
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Payment received. Thank you!");
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateExpenseReceiptAsync(Guid expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId, cancellationToken);
        if (expense == null)
        {
            throw new KeyNotFoundException($"Expense with ID {expenseId} not found");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text("EXPENSE RECEIPT")
                    .SemiBold().FontSize(20);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Receipt #: {expense.Id.ToString().Substring(0, 8)}").SemiBold();
                                col.Item().Text($"Date: {expense.ExpenseDate:yyyy-MM-dd}");
                                col.Item().Text($"Type: {expense.ExpenseType}");
                            });
                        });

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        x.Item().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(150).Text("Category:").SemiBold();
                                row.RelativeItem().Text(expense.Category);
                            });
                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(150).Text("Amount:").SemiBold();
                                row.RelativeItem().Text($"${expense.Amount:F2}").FontSize(14);
                            });
                            if (!string.IsNullOrEmpty(expense.Description))
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantColumn(150).Text("Description:").SemiBold();
                                    row.RelativeItem().Text(expense.Description);
                                });
                            }
                        });
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5)
            .PaddingHorizontal(10);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Payment;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using Infrastructure = PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using System.Security.Claims;
using PaymentIntentDto = PoultryDistributionSystem.Application.DTOs.Payment.PaymentIntentDto;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Payments controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,ShopOwner")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly PoultryDistributionSystem.Infrastructure.Services.Interfaces.IPdfService _pdfService;

    public PaymentsController(IPaymentService paymentService, PoultryDistributionSystem.Infrastructure.Services.Interfaces.IPdfService pdfService)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("sale/{saleId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetBySaleId(Guid saleId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetBySaleIdAsync(saleId, cancellationToken);
        return Ok(ApiResponse<PagedResult<PaymentDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _paymentService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentDto>.SuccessResponse(result, "Payment recorded successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("gateway")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessGatewayPayment([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _paymentService.ProcessPaymentGatewayAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentDto>.SuccessResponse(result, "Payment processed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetMyPayments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
            }

            var result = await _paymentService.GetMyPaymentsAsync(userId, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<PaymentDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("create-intent")]
    [ProducesResponseType(typeof(ApiResponse<PaymentIntentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentIntentDto>>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paymentService.CreatePaymentIntentAsync(request.Amount, request.SaleId, cancellationToken);
            return Ok(ApiResponse<PaymentIntentDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ConfirmPayment(
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _paymentService.ConfirmPaymentAsync(request.PaymentIntentId, request.SaleId, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentDto>.SuccessResponse(result, "Payment confirmed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}/receipt")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceipt(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateReceiptAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"receipt-{id.ToString().Substring(0, 8)}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

/// <summary>
/// Create payment intent request
/// </summary>
public class CreatePaymentIntentRequest
{
    public decimal Amount { get; set; }
    public Guid SaleId { get; set; }
}

/// <summary>
/// Confirm payment request
/// </summary>
public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public Guid SaleId { get; set; }
}

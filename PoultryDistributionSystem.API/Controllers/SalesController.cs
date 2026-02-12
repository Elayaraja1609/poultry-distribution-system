using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Sale;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Sales controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,ShopOwner")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;
    private readonly IPdfService _pdfService;

    public SalesController(ISalesService salesService, IPdfService pdfService)
    {
        _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _salesService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<SaleDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SaleDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _salesService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<SaleDto>>.SuccessResponse(result));
    }

    [HttpGet("shop/{shopId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SaleDto>>>> GetByShop(
        Guid shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _salesService.GetByShopIdAsync(shopId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<SaleDto>>.SuccessResponse(result));
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SaleDto>>>> GetMySales(
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

            var result = await _salesService.GetMySalesAsync(userId, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<SaleDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Create([FromBody] CreateSaleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _salesService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SaleDto>.SuccessResponse(result, "Sale created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}/invoice")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateInvoiceAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"invoice-{id.ToString().Substring(0, 8)}.pdf");
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

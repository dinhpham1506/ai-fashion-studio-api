using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceById;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceByOrder;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceItems;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoicePdf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly ISender _sender;

    public InvoiceController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId
    {
        get
        {
            var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
            return Guid.TryParse(subject, out var userId) ? userId : Guid.Empty;
        }
    }

    private bool IsStaffOrAdmin => User.IsInRole("STAFF") || User.IsInRole("ADMIN");

    [HttpGet("{invoiceId:guid}")]
    public async Task<IActionResult> GetById(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInvoiceByIdQuery(CurrentUserId, IsStaffOrAdmin, invoiceId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInvoiceByOrderQuery(CurrentUserId, IsStaffOrAdmin, orderId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{invoiceId:guid}/items")]
    public async Task<IActionResult> GetItems(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInvoiceItemsQuery(CurrentUserId, IsStaffOrAdmin, invoiceId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{invoiceId:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInvoicePdfQuery(CurrentUserId, IsStaffOrAdmin, invoiceId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }
}

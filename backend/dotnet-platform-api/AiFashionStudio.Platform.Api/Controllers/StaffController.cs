using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Staff.Commands.UpdateOrderStatus;
using AiFashionStudio.Platform.Application.Staff.Queries.GetOrderPrintInfo;
using AiFashionStudio.Platform.Application.Staff.Queries.GetStaffOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

public record UpdateOrderStatusRequest(string ToStatus, string? Note);

/// <summary>
/// Staff Operation Gateway: chặn role STAFF/ADMIN tại đây rồi chuyển tiếp sang Java Order Service.
/// C# không đọc/ghi trực tiếp bảng orders của Java (service boundary rule).
/// </summary>
[ApiController]
[Route("api/staff")]
[Authorize(Roles = "STAFF,ADMIN")]
public class StaffController : ControllerBase
{
    private readonly ISender _sender;

    public StaffController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Staff xem danh sách đơn hàng cần xử lý, lọc theo trạng thái (mặc định PAID).
    /// </summary>
    /// <param name="status">PAID, IN_PRODUCTION, SHIPPING, COMPLETED, CANCELLED.</param>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status = "PAID",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetStaffOrdersQuery(status, page, pageSize), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// Staff lấy thông tin in ấn của một đơn: snapshot sản phẩm, variant, số lượng,
    /// ảnh preview và print file URL của design gốc.
    /// </summary>
    /// <param name="orderId">ID đơn hàng bên Java Order Service.</param>
    [HttpGet("orders/{orderId:guid}/print-info")]
    public async Task<IActionResult> GetPrintInfo(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOrderPrintInfoQuery(orderId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// Staff chuyển trạng thái đơn: PAID → IN_PRODUCTION → SHIPPING → COMPLETED.
    /// Gateway chỉ chuyển tiếp — Java Order Service validate transition và ghi lịch sử.
    /// </summary>
    /// <param name="orderId">ID đơn hàng bên Java Order Service.</param>
    [HttpPatch("orders/{orderId:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateOrderStatusCommand(orderId, request.ToStatus, request.Note, CurrentUserId),
            cancellationToken);
        return Ok(ApiResponse.Ok(result, "Order status updated"));
    }

    private Guid CurrentUserId
    {
        get
        {
            var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
            return Guid.TryParse(subject, out var userId) ? userId : Guid.Empty;
        }
    }
}

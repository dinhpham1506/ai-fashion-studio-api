using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Staff.Commands.UpdateOrderStatus;

/// <summary>
/// Staff yêu cầu chuyển trạng thái đơn hàng. Gateway chuyển tiếp sang Java Order Service —
/// Java giữ state machine và ghi order_status_history.
/// </summary>
public record UpdateOrderStatusCommand(
    Guid OrderId,
    string ToStatus,
    string? Note,
    Guid ChangedBy) : IRequest<UpdateOrderStatusResponse>;

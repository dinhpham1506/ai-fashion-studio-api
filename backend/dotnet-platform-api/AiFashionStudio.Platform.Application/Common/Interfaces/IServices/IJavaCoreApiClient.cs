using AiFashionStudio.Platform.Application.Common.Dtos;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

/// <summary>
/// Client gọi sang Java Core API (Order/Design service).
/// Java owns bảng orders — C# không đọc/ghi trực tiếp bảng của Java mà đi qua internal API:
///   GET /api/internal/orders?status=&amp;page=&amp;pageSize=
///   GET /api/internal/orders/{orderId}/print-info
/// </summary>
public interface IJavaCoreApiClient
{
    /// <summary>Lấy danh sách đơn hàng cho Staff, lọc theo trạng thái.</summary>
    Task<StaffOrderListResponse> GetStaffOrdersAsync(
        string? status, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Lấy thông tin in ấn (snapshot sản phẩm + print file URL) của một đơn.</summary>
    Task<PrintInfoResponse> GetOrderPrintInfoAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Yêu cầu Java cập nhật trạng thái đơn (PAID → IN_PRODUCTION → SHIPPING → COMPLETED).
    /// Java owns order lifecycle — gateway chỉ chuyển tiếp, không tự đổi trạng thái.
    /// </summary>
    Task<UpdateOrderStatusResponse> UpdateOrderStatusAsync(
        Guid orderId, string toStatus, string? note, Guid changedBy, CancellationToken cancellationToken);
}

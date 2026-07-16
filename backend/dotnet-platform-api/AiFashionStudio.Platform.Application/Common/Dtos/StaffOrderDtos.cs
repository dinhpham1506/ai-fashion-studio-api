namespace AiFashionStudio.Platform.Application.Common.Dtos;

/// <summary>Một đơn hàng trong danh sách Staff cần xử lý (dữ liệu lấy từ Java Order Service).</summary>
public record StaffOrderResponse(
    Guid OrderId,
    string OrderCode,
    Guid CustomerId,
    decimal TotalAmount,
    string OrderStatus,
    string PaymentStatus,
    DateTime CreatedAt);

/// <summary>Danh sách đơn hàng có phân trang cho Staff.</summary>
public record StaffOrderListResponse(
    IReadOnlyList<StaffOrderResponse> Items,
    int Page,
    int PageSize,
    long TotalItems,
    int TotalPages);

/// <summary>Size/màu/chất liệu snapshot tại thời điểm đặt hàng.</summary>
public record PrintVariantResponse(string Size, string Color, string Material);

/// <summary>Một item cần in: snapshot sản phẩm + file in từ design gốc (không dùng ảnh Try-On).</summary>
public record PrintInfoItemResponse(
    Guid OrderItemId,
    string ProductName,
    PrintVariantResponse Variant,
    int Quantity,
    Guid DesignId,
    string? PreviewImageUrl,
    string? PrintFileUrl);

/// <summary>Thông tin in ấn của một đơn hàng cho Staff.</summary>
public record PrintInfoResponse(
    Guid OrderId,
    string OrderCode,
    IReadOnlyList<PrintInfoItemResponse> Items);

/// <summary>Kết quả sau khi Staff cập nhật trạng thái đơn (Java thực hiện, gateway chuyển tiếp).</summary>
public record UpdateOrderStatusResponse(
    Guid OrderId,
    string FromStatus,
    string ToStatus);

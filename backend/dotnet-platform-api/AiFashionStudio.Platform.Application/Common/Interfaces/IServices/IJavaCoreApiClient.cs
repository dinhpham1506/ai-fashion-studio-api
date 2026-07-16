using AiFashionStudio.Platform.Application.Common.Dtos;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

/// <summary>
/// Client used by the .NET platform API to call Java Core API.
/// Java owns catalog/order data, so .NET talks to Java through internal/public APIs instead of reading Java tables directly.
/// </summary>
public interface IJavaCoreApiClient
{
    Task<IReadOnlyList<CatalogProductResponse>> SearchPublicProductsAsync(
        string? name, CancellationToken cancellationToken);

    Task<ProductDetailResponse> GetPublicProductDetailAsync(
        Guid productId, CancellationToken cancellationToken);

    Task<OrderDetailResponse> GetOrderDetailAsync(
        Guid orderId, Guid requesterId, string? userRole, CancellationToken cancellationToken);

    Task<StaffOrderListResponse> GetStaffOrdersAsync(
        string? status, int page, int pageSize, CancellationToken cancellationToken);

    Task<PrintInfoResponse> GetOrderPrintInfoAsync(Guid orderId, CancellationToken cancellationToken);

    Task<UpdateOrderStatusResponse> UpdateOrderStatusAsync(
        Guid orderId, string toStatus, string? note, Guid changedBy, CancellationToken cancellationToken);
}

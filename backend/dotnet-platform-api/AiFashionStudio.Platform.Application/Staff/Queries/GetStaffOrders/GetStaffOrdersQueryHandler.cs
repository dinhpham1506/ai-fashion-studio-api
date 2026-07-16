using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;

namespace AiFashionStudio.Platform.Application.Staff.Queries.GetStaffOrders;

public class GetStaffOrdersQueryHandler : IRequestHandler<GetStaffOrdersQuery, StaffOrderListResponse>
{
    private readonly IJavaCoreApiClient _javaCoreApiClient;

    public GetStaffOrdersQueryHandler(IJavaCoreApiClient javaCoreApiClient)
    {
        _javaCoreApiClient = javaCoreApiClient;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng cho Staff từ Java Order Service.
    /// Role STAFF/ADMIN đã được chặn ở controller — handler chỉ lo chuyển tiếp truy vấn.
    /// </summary>
    public async Task<StaffOrderListResponse> Handle(GetStaffOrdersQuery request, CancellationToken cancellationToken)
    {
        var status = request.Status?.ToUpperInvariant();
        return await _javaCoreApiClient.GetStaffOrdersAsync(status, request.Page, request.PageSize, cancellationToken);
    }
}

using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Staff.Queries.GetStaffOrders;

/// <summary>Staff/Admin xem danh sách đơn hàng cần xử lý (dữ liệu từ Java Order Service).</summary>
public record GetStaffOrdersQuery(string? Status, int Page, int PageSize)
    : IRequest<StaffOrderListResponse>;

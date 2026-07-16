using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Staff.Queries.GetOrderPrintInfo;

/// <summary>Staff/Admin lấy thông tin in ấn của một đơn hàng (dữ liệu từ Java Order/Design Service).</summary>
public record GetOrderPrintInfoQuery(Guid OrderId) : IRequest<PrintInfoResponse>;

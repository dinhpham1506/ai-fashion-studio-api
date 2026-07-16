using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;

namespace AiFashionStudio.Platform.Application.Staff.Queries.GetOrderPrintInfo;

public class GetOrderPrintInfoQueryHandler : IRequestHandler<GetOrderPrintInfoQuery, PrintInfoResponse>
{
    private readonly IJavaCoreApiClient _javaCoreApiClient;

    public GetOrderPrintInfoQueryHandler(IJavaCoreApiClient javaCoreApiClient)
    {
        _javaCoreApiClient = javaCoreApiClient;
    }

    /// <summary>
    /// Lấy print info từ Java. File in luôn là design.printFileUrl —
    /// không bao giờ dùng ảnh Try-On làm file in (BR-005).
    /// </summary>
    public async Task<PrintInfoResponse> Handle(GetOrderPrintInfoQuery request, CancellationToken cancellationToken)
    {
        return await _javaCoreApiClient.GetOrderPrintInfoAsync(request.OrderId, cancellationToken);
    }
}

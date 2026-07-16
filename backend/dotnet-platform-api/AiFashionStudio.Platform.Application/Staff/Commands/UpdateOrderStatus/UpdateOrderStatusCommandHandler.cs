using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;

namespace AiFashionStudio.Platform.Application.Staff.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResponse>
{
    private readonly IJavaCoreApiClient _javaCoreApiClient;

    public UpdateOrderStatusCommandHandler(IJavaCoreApiClient javaCoreApiClient)
    {
        _javaCoreApiClient = javaCoreApiClient;
    }

    /// <summary>
    /// Chuyển tiếp yêu cầu đổi trạng thái sang Java. Java validate transition
    /// (PAID → IN_PRODUCTION → SHIPPING → COMPLETED) và ghi lịch sử.
    /// </summary>
    public async Task<UpdateOrderStatusResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        return await _javaCoreApiClient.UpdateOrderStatusAsync(
            request.OrderId,
            request.ToStatus.ToUpperInvariant(),
            request.Note,
            request.ChangedBy,
            cancellationToken);
    }
}

using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;


namespace AiFashionStudio.Platform.Application.Payments.Commands.CancelPayment
{
    public class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand>
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentOrderRepository _paymentOrderRepository;

        public CancelPaymentCommandHandler(IPaymentGatewayService paymentGatewayService,
            IPaymentOrderRepository paymentOrderRepository)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentOrderRepository = paymentOrderRepository;
        }

        public async Task Handle(CancelPaymentCommand command, CancellationToken cancellationToken)
        {
            var order = await _paymentOrderRepository.GetByOrderCodeAndUserIdAsync(command.OrderCode, command.UserId, cancellationToken) ?? throw new NotFoundException("PAYMENT_NOT_FOUND", "Payment not found");

            if (!order.IsPending())
            {
                throw new ConflictException("PAYMENT_NOT_CANCELLABLE", "Only pending payments can be cancelled");
            }

            await _paymentGatewayService.CancelPaymentLinkAsync(command.OrderCode, "Cancel By Customer!", cancellationToken);

            order.Cancel();
            await _paymentOrderRepository.SaveChangesAsync(cancellationToken);

        }
    }

}

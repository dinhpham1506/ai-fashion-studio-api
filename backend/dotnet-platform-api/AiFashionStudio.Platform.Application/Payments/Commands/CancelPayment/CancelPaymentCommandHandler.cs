using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Payment.Enums;
using MediatR;

namespace AiFashionStudio.Platform.Application.Payments.Commands.CancelPayment
{
    public class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand>
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentOrderRepository _paymentOrderRepository;

        public CancelPaymentCommandHandler(
            IPaymentGatewayService paymentGatewayService,
            IPaymentOrderRepository paymentOrderRepository)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentOrderRepository = paymentOrderRepository;
        }

        /// <summary>
        /// Cancels a pending payment order.
        /// </summary>
        /// <param name="command">The cancellation command containing the order code and user identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="NotFoundException">Thrown when the payment order cannot be found.</exception>
        /// <exception cref="ConflictException">Thrown when the payment order is not pending.</exception>
        public async Task Handle(CancelPaymentCommand command, CancellationToken cancellationToken)
        {
            var order = await _paymentOrderRepository.GetByOrderCodeAndUserIdAsync(
                command.OrderCode,
                command.UserId,
                cancellationToken) ?? throw new NotFoundException("PAYMENT_NOT_FOUND", "Payment not found");

            if (!order.IsPending() && !order.IsCancellationRequested())
            {
                throw new ConflictException("PAYMENT_NOT_CANCELLABLE", "Only pending payments can be cancelled");
            }

            if (order.IsPending())
            {
                order.MarkCancellationRequested();
                await _paymentOrderRepository.SaveChangesAsync(cancellationToken);
            }

            var gatewayStatus = await _paymentGatewayService.GetPaymentLinkStatusAsync(command.OrderCode, cancellationToken);
            if (!string.Equals(gatewayStatus, PaymentStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                await _paymentGatewayService.CancelPaymentLinkAsync(command.OrderCode, null, cancellationToken);
            }

            order.Cancel();
            await _paymentOrderRepository.SaveChangesAsync(cancellationToken);
        }
    }
}

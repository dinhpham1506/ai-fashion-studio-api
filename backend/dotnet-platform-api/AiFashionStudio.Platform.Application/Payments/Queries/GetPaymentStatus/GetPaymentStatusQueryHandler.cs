using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AiFashionStudio.Platform.Application.Payments.Queries.GetPaymentStatus
{
    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, PaymentStatusResponse>
    {
        private readonly IPaymentOrderRepository _paymentOrderRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPaymentStatusQueryHandler"/> class.
        /// </summary>
        public GetPaymentStatusQueryHandler(IPaymentOrderRepository paymentOrderRepository)
        {
            _paymentOrderRepository = paymentOrderRepository;
        }

        /// <summary>
        /// Gets the payment status for the specified payment or order.
        /// </summary>
        /// <param name="query">The payment lookup criteria.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The payment status response for the matching payment.</returns>
        /// <exception cref="NotFoundException">Thrown when no payment matches the specified criteria and user ID.</exception>
        public async Task<PaymentStatusResponse> Handle(GetPaymentStatusQuery query, CancellationToken cancellationToken)
        {
            var order = query.PaymentId.HasValue
                ? await _paymentOrderRepository.GetByIdAndUserIdAsync(query.PaymentId.Value, query.UserId, cancellationToken)
                : query.OrderId.HasValue
                    ? await _paymentOrderRepository.GetByOrderIdAndUserIdAsync(query.OrderId.Value, query.UserId, cancellationToken)
                    : null;

            if (order is null)
            {
                throw new NotFoundException("PAYMENT_NOT_FOUND", "Payment order not found");
            }

            return new PaymentStatusResponse(
                order.Id,
                order.OrderId,
                order.OrderCode,
                order.Amount,
                order.Status.ToString().ToUpperInvariant(),
                order.CreatedAt,
                order.PaidAt);
        }
    }
}

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

        public GetPaymentStatusQueryHandler(IPaymentOrderRepository paymentOrderRepository)
        {
            _paymentOrderRepository = paymentOrderRepository;
        }

        public async Task<PaymentStatusResponse> Handle(GetPaymentStatusQuery query, CancellationToken cancellationToken)
        {
            var order = await _paymentOrderRepository.GetByOrderCodeAndUserIdAsync(query.OrderCode, query.UserId, cancellationToken)
                ?? throw new NotFoundException("PAYMENT_NOT_FOUND", "Payment order not found");

            return new PaymentStatusResponse(
                order.Id,
                order.OrderCode,
                order.Amount,
                order.Status.ToString().ToUpperInvariant(),
                order.CreatedAt,
                order.PaidAt);
        }
    }
}

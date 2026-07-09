using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Models;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentLinkResponse>
    {
        private readonly IPaymentOrderRepository _paymentOrderRepository;
        private readonly IPaymentGatewayService _gatewayService;
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePaymentCommandHandler"/> class.
        /// </summary>
        public CreatePaymentCommandHandler(IPaymentOrderRepository paymentOrderRepository, IPaymentGatewayService paymentGatewayService)
        {
            _paymentOrderRepository = paymentOrderRepository;
            _gatewayService = paymentGatewayService;
        }
        /// <summary>
        /// Creates a payment order and returns the payment link details.
        /// </summary>
        /// <param name="command">The payment details and user identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created order code, checkout URL, QR code, amount, and order status.</returns>
        public async Task<CreatePaymentLinkResponse> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            // Sinh orderCode:
            // 1. UnixTimeMilliseconds() = số mili-giây từ epoch (hiện ~13 chữ số) — tăng dần theo thời gian, dễ debug/sort.
            // 2. * 1000 = dịch trái 3 chữ số, chừa chỗ cho phần random ở cuối (ra số ~16 chữ số).
            // 3. + RandomNumberGenerator.GetInt32(0, 1000) = cộng số random 0-999 (crypto-secure) vào 3 chữ số cuối,
            //    để 2 request tạo cùng lúc trong 1 mili-giây không bị trùng orderCode.
            // Kết quả luôn < 9_007_199_254_740_991 (giới hạn PayOS/safe-integer), unique index ở DB chặn nốt trường hợp trùng còn sót.
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + RandomNumberGenerator.GetInt32(0, 1000);

            var order = PaymentOrder.Create(command.UserId, orderCode, command.Amount, command.Description);

            await _paymentOrderRepository.AddAsync(order, cancellationToken);

            var link = await _gatewayService.CreatePaymentLinkAsync(new PaymentLinkRequest(orderCode, command.Amount, command.Description, ReturnUrl: "", CancelUrl: ""));

            order.AttachPaymentLink(link.PaymentLink);
            await _paymentOrderRepository.SaveChangesAsync(cancellationToken);

            return new CreatePaymentLinkResponse(orderCode, link.CheckOutUrl, link.QrCode, command.Amount, order.Status.ToString().ToUpperInvariant());


        }
    }
}

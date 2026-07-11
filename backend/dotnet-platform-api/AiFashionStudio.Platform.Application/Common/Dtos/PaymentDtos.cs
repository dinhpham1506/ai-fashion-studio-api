using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Dtos;

public record CreatePaymentLinkResponse(long OrderCode, string CheckoutUrl, string QrCode, int Amount, string Status);

public record PaymentStatusResponse(Guid OrderId, long OrderCode, int Amount, string Status, DateTime CreatedAt, DateTime? PaidAt);

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Models
{
    // 
    public record PaymentLinkRequest(long OrderCode, int Amount, string Description, string ReturnUrl, string CancelUrl);
    
    public record PaymentLinkResponse(string PaymentLink, string CheckOutUrl, string QrCode);

    public record PaymentWebhookData(long OrderCode, long Amount, string Reference, bool IsSuccess);
}

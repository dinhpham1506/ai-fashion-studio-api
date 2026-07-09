using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Payment
{
    public class PayOsSettings
    {
        public const string SectionName = "PayOs";

        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;   // FE page hiển thị "thanh toán thành công"
        public string CancelUrl { get; set; } = string.Empty;   // FE page hiển thị "đã hủy"
    }
}

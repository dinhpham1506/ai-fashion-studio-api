using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;

namespace AiFashionStudio.Platform.Application.AiChat;

/// <summary>
/// Rule-based safety layer that runs after the single LLM generator.
/// It keeps sales/support replies inside verified product, order, payment and policy facts.
/// </summary>
public static class AiChatPolicyGuard
{
    private static readonly string[] ForbiddenClaims =
    {
        "giảm giá",
        "giam gia",
        "hoàn tiền chắc chắn",
        "hoan tien chac chan",
        "cam kết giao",
        "cam ket giao",
        "miễn phí",
        "mien phi"
    };

    public static AiChatResponse Apply(AiChatResponse response)
    {
        var reply = response.Reply.Trim();
        if (ContainsUnsupportedClaim(reply) && !HasVerifiedCommercialFact(response))
        {
            reply = "Em sẽ không tự hứa giảm giá, miễn phí hay cam kết ngoài chính sách hệ thống. Em có thể kiểm tra sản phẩm, giỏ hàng, thanh toán hoặc tạo yêu cầu hỗ trợ để nhân viên xác nhận chính xác cho anh/chị.";
        }

        if (reply.Length > 900)
        {
            reply = reply[..900].Trim();
        }

        return response with { Reply = reply };
    }

    public static void EnsureAuthenticatedForPrivateData(Guid? userId)
    {
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("Login is required to access private order, payment or cart data.");
        }
    }

    private static bool ContainsUnsupportedClaim(string reply)
    {
        var normalized = reply.ToLowerInvariant();
        return ForbiddenClaims.Any(claim => normalized.Contains(claim, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasVerifiedCommercialFact(AiChatResponse response)
    {
        return response.Intent is "PRODUCT_SEARCH" or "PRODUCT_DETAIL_HELP"
               && response.Cards.Any();
    }
}

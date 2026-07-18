using AiFashionStudio.Platform.Application.AiChat;
using AiFashionStudio.Platform.Application.Common.Dtos;
using Xunit;

namespace AiFashionStudio.Platform.Tests.AiChat;

public class AiChatPolicyGuardTests
{
    [Fact]
    public void Apply_Should_Block_Unverified_Commercial_Promise()
    {
        var response = new AiChatResponse(
            Guid.NewGuid(),
            "Em cam kết giao trong hôm nay và giảm giá cho anh/chị.",
            "GENERAL_HELP",
            Array.Empty<AiChatProductCard>(),
            Array.Empty<string>());

        var guarded = AiChatPolicyGuard.Apply(response);

        Assert.Contains("không tự hứa", guarded.Reply);
    }

    [Fact]
    public void Apply_Should_Keep_Grounded_Product_Response()
    {
        var productId = Guid.NewGuid();
        var response = new AiChatResponse(
            Guid.NewGuid(),
            "Em tìm được mẫu này đúng nhu cầu của anh/chị.",
            "PRODUCT_SEARCH",
            new[]
            {
                new AiChatProductCard(
                    "PRODUCT",
                    productId,
                    "Áo thun basic",
                    199000,
                    null,
                    $"/products/{productId}",
                    new[] { "M", "L" },
                    new[] { "black" })
            },
            new[] { "Tư vấn size" });

        var guarded = AiChatPolicyGuard.Apply(response);

        Assert.Equal(response.Reply, guarded.Reply);
    }
}

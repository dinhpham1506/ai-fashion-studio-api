using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.AiChat.Entities;
using AiFashionStudio.Platform.Domain.AiChat.Enums;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.StartAiChatConversation;

public class StartAiChatConversationCommandHandler : IRequestHandler<StartAiChatConversationCommand, AiChatResponse>
{
    private const string OrderProcessingIssuePrompt =
        "Nếu anh/chị đang gặp vấn đề khi xử lý order, ví dụ thanh toán xong nhưng đơn chưa cập nhật, link thanh toán lỗi hoặc đơn bị kẹt, em có thể kiểm tra và tạo yêu cầu hỗ trợ cho mình.";

    private const string OrderProcessingIssuePromptEn =
        "If you have an order processing issue, for example payment was completed but the order has not updated, the payment link failed, or the order is stuck, I can check it and create a support request for you.";

    private readonly IBaseRepository<AiChatConversation> _conversationRepository;
    private readonly IBaseRepository<AiChatMessage> _messageRepository;

    public StartAiChatConversationCommandHandler(
        IBaseRepository<AiChatConversation> conversationRepository,
        IBaseRepository<AiChatMessage> messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<AiChatResponse> Handle(StartAiChatConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = AiChatConversation.Start(
            request.UserId,
            request.UserRole,
            request.Channel,
            request.Page?.Type,
            request.Page?.ProductId,
            request.Page?.OrderId);

        await _conversationRepository.AddAsync(conversation, cancellationToken);

        var language = ResolveLanguage(request.Page);
        var (reply, intent, suggestions) = BuildGreeting(request.Page?.Type, language);
        await _messageRepository.AddAsync(
            AiChatMessage.Create(conversation.Id, AiChatSenderType.Assistant, reply, intent),
            cancellationToken);

        return new AiChatResponse(
            conversation.Id,
            reply,
            intent,
            Array.Empty<AiChatProductCard>(),
            suggestions)
        {
            Actions = BuildGreetingActions(intent, request.Page, language)
        };
    }

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) BuildGreeting(string? pageType, string language)
    {
        var english = IsEnglish(language);

        return NormalizePageType(pageType) switch
        {
            "PRODUCT_DETAIL" when english => (
                "You are viewing this product. What would you like help with: size, material, available colors, or styling ideas?",
                "PRODUCT_DETAIL_HELP",
                new[] { "Recommend my size", "Are there other colors?", "Suggest styling ideas" }),
            "PRODUCT_DETAIL" => ProductDetailGreetingVi(),
            "CART" when english => (
                $"Would you like me to check the sizes, colors, and items in your cart before you order? {OrderProcessingIssuePromptEn}",
                "CART_ADVICE",
                new[] { "Check sizes", "Any item out of stock?", "I have an order issue" }),
            "CART" => CartGreetingVi(),
            "CHECKOUT" when english => (
                $"Do you need help with payment or checking your order details before paying? {OrderProcessingIssuePromptEn}",
                "CHECKOUT_HELP",
                new[] { "Paid but not updated", "Payment link failed", "Check my order" }),
            "CHECKOUT" => CheckoutGreetingVi(),
            "ORDER_DETAIL" when english => (
                $"Would you like me to check the order or payment status for this order? {OrderProcessingIssuePromptEn}",
                "ORDER_STATUS_HELP",
                new[] { "Order is stuck", "Paid but not updated", "Create support request" }),
            "ORDER_DETAIL" => OrderDetailGreetingVi(),
            _ when english => (
                "What product are you looking for today?",
                "PRODUCT_SEARCH",
                new[] { "Find party dresses", "Find office shirts", "Find products under 500k" }),
            _ => ProductSearchGreetingVi()
        };
    }

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) ProductDetailGreetingVi()
        => (
            "Anh/chị đang xem sản phẩm này. Anh/chị cần em tư vấn gì: size, chất liệu, màu còn hàng hay cách phối đồ ạ?",
            "PRODUCT_DETAIL_HELP",
            new[] { "Tư vấn size", "Có màu nào khác không?", "Gợi ý phối đồ" });

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) CartGreetingVi()
        => (
            $"Anh/chị muốn em kiểm tra lại size, màu và sản phẩm trong giỏ trước khi đặt không ạ? {OrderProcessingIssuePrompt}",
            "CART_ADVICE",
            new[] { "Kiểm tra size", "Có sản phẩm nào hết hàng không?", "Tôi gặp lỗi xử lý đơn" });

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) CheckoutGreetingVi()
        => (
            $"Anh/chị cần em hướng dẫn thanh toán hoặc kiểm tra thông tin đơn trước khi trả tiền không ạ? {OrderProcessingIssuePrompt}",
            "CHECKOUT_HELP",
            new[] { "Đã thanh toán nhưng chưa cập nhật", "Link thanh toán bị lỗi", "Kiểm tra đơn hàng" });

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) OrderDetailGreetingVi()
        => (
            $"Anh/chị muốn em kiểm tra trạng thái đơn hàng hoặc thanh toán của đơn này không ạ? {OrderProcessingIssuePrompt}",
            "ORDER_STATUS_HELP",
            new[] { "Đơn bị kẹt xử lý", "Đã thanh toán nhưng chưa cập nhật", "Tạo yêu cầu hỗ trợ" });

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) ProductSearchGreetingVi()
        => (
            "Anh/chị đang tìm sản phẩm gì hôm nay ạ?",
            "PRODUCT_SEARCH",
            new[] { "Tìm váy đi tiệc", "Tìm áo công sở", "Tìm sản phẩm dưới 500k" });

    private static string? NormalizePageType(string? pageType)
        => string.IsNullOrWhiteSpace(pageType) ? null : pageType.Trim().ToUpperInvariant();

    private static IReadOnlyCollection<AiChatAction> BuildGreetingActions(string intent, AiChatPageContext? page, string language)
    {
        var english = IsEnglish(language);
        var productId = page?.ProductId;
        var orderId = page?.OrderId;

        return intent switch
        {
            "PRODUCT_DETAIL_HELP" when productId.HasValue => new[]
            {
                Navigate(english ? "Open current product" : "Mở sản phẩm đang xem", $"/products/{productId}", $"/api/products/{productId}", "GET"),
                Navigate(english ? "Find products" : "Tìm sản phẩm", "/products", "/api/products", "GET")
            },
            "CART_ADVICE" => new[]
            {
                Navigate(english ? "Open cart" : "Mở giỏ hàng", "/cart", "/api/cart", "GET"),
                Navigate(english ? "Checkout cart" : "Checkout giỏ hàng", "/checkout", "/api/cart/checkout", "POST")
            },
            "CHECKOUT_HELP" => new[]
            {
                Navigate(english ? "Open checkout" : "Mở checkout", "/checkout"),
                Navigate(english ? "View orders" : "Xem đơn hàng", "/orders", "/api/orders/my", "GET")
            },
            "ORDER_STATUS_HELP" when orderId.HasValue => new[]
            {
                Navigate(english ? "Open order details" : "Mở chi tiết đơn hàng", $"/orders/{orderId}", $"/api/orders/{orderId}", "GET"),
                new AiChatAction(english ? "Create support request" : "Tạo yêu cầu hỗ trợ", "SUPPORT_HANDOFF")
            },
            "ORDER_STATUS_HELP" => new[]
            {
                Navigate(english ? "Open order list" : "Mở danh sách đơn hàng", "/orders", "/api/orders/my", "GET"),
                new AiChatAction(english ? "Create support request" : "Tạo yêu cầu hỗ trợ", "SUPPORT_HANDOFF")
            },
            _ => new[]
            {
                Navigate(english ? "Find products" : "Tìm sản phẩm", "/products", "/api/products", "GET")
            }
        };
    }

    private static AiChatAction Navigate(string label, string navigateUrl, string? apiRoute = null, string? method = null)
        => new(label, "NAVIGATE", navigateUrl, apiRoute, method);

    private static string ResolveLanguage(AiChatPageContext? page)
        => NormalizeLanguage(page?.Language) ?? "VI";

    private static string? NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var normalized = language.Trim().ToUpperInvariant();
        return normalized switch
        {
            "EN" or "ENG" or "EN-US" or "ENGLISH" => "EN",
            "VI" or "VN" or "VI-VN" or "VIETNAMESE" or "TIENG VIET" => "VI",
            _ => null
        };
    }

    private static bool IsEnglish(string language)
        => string.Equals(language, "EN", StringComparison.OrdinalIgnoreCase);
}

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

        var (reply, intent, suggestions) = BuildGreeting(request.Page?.Type);
        await _messageRepository.AddAsync(
            AiChatMessage.Create(conversation.Id, AiChatSenderType.Assistant, reply, intent),
            cancellationToken);

        return new AiChatResponse(
            conversation.Id,
            reply,
            intent,
            Array.Empty<AiChatProductCard>(),
            suggestions);
    }

    private static (string Reply, string Intent, IReadOnlyCollection<string> Suggestions) BuildGreeting(string? pageType)
    {
        return NormalizePageType(pageType) switch
        {
            "PRODUCT_DETAIL" => (
                "Anh/chị đang xem sản phẩm này. Anh/chị cần em tư vấn gì: size, chất liệu, màu còn hàng hay cách phối đồ ạ?",
                "PRODUCT_DETAIL_HELP",
                new[] { "Tư vấn size", "Có màu nào khác không?", "Gợi ý phối đồ" }),
            "CART" => (
                $"Anh/chị muốn em kiểm tra lại size, màu và sản phẩm trong giỏ trước khi đặt không ạ? {OrderProcessingIssuePrompt}",
                "CART_ADVICE",
                new[] { "Kiểm tra size", "Có sản phẩm nào hết hàng không?", "Tôi gặp lỗi xử lý đơn" }),
            "CHECKOUT" => (
                $"Anh/chị cần em hướng dẫn thanh toán hoặc kiểm tra thông tin đơn trước khi trả tiền không ạ? {OrderProcessingIssuePrompt}",
                "CHECKOUT_HELP",
                new[] { "Đã thanh toán nhưng chưa cập nhật", "Link thanh toán bị lỗi", "Kiểm tra đơn hàng" }),
            "ORDER_DETAIL" => (
                $"Anh/chị muốn em kiểm tra trạng thái đơn hàng hoặc thanh toán của đơn này không ạ? {OrderProcessingIssuePrompt}",
                "ORDER_STATUS_HELP",
                new[] { "Đơn bị kẹt xử lý", "Đã thanh toán nhưng chưa cập nhật", "Tạo yêu cầu hỗ trợ" }),
            _ => (
                "Anh/chị đang tìm sản phẩm gì hôm nay ạ?",
                "PRODUCT_SEARCH",
                new[] { "Tìm váy đi tiệc", "Tìm áo công sở", "Tìm sản phẩm dưới 500k" })
        };
    }

    private static string? NormalizePageType(string? pageType)
        => string.IsNullOrWhiteSpace(pageType) ? null : pageType.Trim().ToUpperInvariant();
}

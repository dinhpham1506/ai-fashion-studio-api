using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AiFashionStudio.Platform.Application.AiChat;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.AiChat.Entities;
using AiFashionStudio.Platform.Domain.AiChat.Enums;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.SendAiChatMessage;

public partial class SendAiChatMessageCommandHandler : IRequestHandler<SendAiChatMessageCommand, AiChatResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string OrderProcessingIssuePrompt =
        "Nếu anh/chị thấy đơn bị kẹt xử lý, đã thanh toán nhưng chưa cập nhật, hoặc cần nhân viên kiểm tra, anh/chị nói em biết vấn đề đang gặp để em tạo yêu cầu hỗ trợ nhé.";

    private readonly IBaseRepository<AiChatConversation> _conversationRepository;
    private readonly IBaseRepository<AiChatMessage> _messageRepository;
    private readonly IBaseRepository<AiChatToolRun> _toolRunRepository;
    private readonly IBaseRepository<AiChatSupportTicket> _supportTicketRepository;
    private readonly IJavaCoreApiClient _javaCoreApiClient;
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IGeminiChatClient _geminiChatClient;

    private const string ReplyRefinementSystemInstruction =
        "Bạn là nhân viên bán hàng thời trang giỏi nhất của AI Fashion Studio, xưng \"em\", gọi khách là \"anh/chị\". " +
        "Mục tiêu cao nhất của bạn là thuyết phục khách chốt đơn: luôn chủ động, nhiệt tình, khen khéo lựa chọn của khách và làm nổi bật ưu điểm của sản phẩm trong dữ liệu JSON. " +
        "Bạn sẽ nhận tin nhắn của khách, lịch sử chat gần nhất và dữ liệu JSON đã được hệ thống kiểm tra sẵn. " +
        "Cách bán hàng: phân tích nhu cầu của khách trong phạm vi dữ liệu JSON, tư vấn như một người bán tận tâm, và luôn kết thúc bằng một lời mời hành động cụ thể như chốt size, thêm vào giỏ hoặc thanh toán ngay. " +
        "Nếu khách phân vân, hãy xử lý băn khoăn đó và đưa lý do thuyết phục dựa trên dữ liệu thật (ví dụ size/màu còn hàng thì gợi ý chốt sớm kẻo hết size, sản phẩm hợp dáng người thì nhấn mạnh điểm hợp). " +
        "Nếu JSON đã có recommendation thì phải dùng đúng recommendation đó và bán theo hướng đề xuất này; có thể giải thích ngắn vì sao đề xuất như vậy. " +
        "Nếu thiếu dữ liệu bắt buộc thì hỏi đúng phần còn thiếu một cách khéo léo để dẫn khách tiếp tục mua, nhưng không hỏi lại thông tin khách đã cung cấp trong tin nhắn hoặc lịch sử gần nhất. " +
        "Tuyệt đối không bịa thêm sản phẩm, size, giá, khuyến mãi, trạng thái đơn hàng hay thời gian giao hàng ngoài dữ liệu JSON được cung cấp; chỉ được tạo cảm giác khan hiếm khi dữ liệu tồn kho thật cho phép. " +
        "Không tự ý hứa giảm giá. Trả lời tối đa 4 câu, không dùng markdown, không liệt kê lại JSON, chỉ trả về đoạn văn trả lời cuối cùng.";

    public SendAiChatMessageCommandHandler(
        IBaseRepository<AiChatConversation> conversationRepository,
        IBaseRepository<AiChatMessage> messageRepository,
        IBaseRepository<AiChatToolRun> toolRunRepository,
        IBaseRepository<AiChatSupportTicket> supportTicketRepository,
        IJavaCoreApiClient javaCoreApiClient,
        IPaymentOrderRepository paymentOrderRepository,
        IGeminiChatClient geminiChatClient)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _toolRunRepository = toolRunRepository;
        _supportTicketRepository = supportTicketRepository;
        _javaCoreApiClient = javaCoreApiClient;
        _paymentOrderRepository = paymentOrderRepository;
        _geminiChatClient = geminiChatClient;
    }

    public async Task<AiChatResponse> Handle(SendAiChatMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("AI_CHAT_CONVERSATION_NOT_FOUND", "AI chat conversation not found");

        AiChatConversationAccess.EnsureCanAccess(conversation, request.UserId);

        conversation.TouchContext(request.Page?.Type, request.Page?.ProductId, request.Page?.OrderId);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        await _messageRepository.AddAsync(
            AiChatMessage.Create(
                conversation.Id,
                AiChatSenderType.User,
                request.Message,
                metadataJson: SerializeOrNull(new { request.Page, request.ClientContext })),
            cancellationToken);

        var intentProfile = DetectIntentProfile(request);
        var intent = intentProfile.Intent;
        var verifiedMemory = await BuildVerifiedMemoryAsync(request, cancellationToken);
        var response = intent switch
        {
            "ADD_TO_CART" => await HandleAddToCartAsync(conversation.Id, request, cancellationToken),
            "CART_CHECKOUT" => await HandleCartCheckoutAsync(conversation.Id, request, cancellationToken),
            "CART_STATUS" => await HandleCartStatusAsync(conversation.Id, request, cancellationToken),
            "CHECKOUT_HELP" => BuildCheckoutHelpResponse(conversation.Id),
            "SIZE_ADVICE" => await HandleSizeAdviceAsync(conversation.Id, request, cancellationToken),
            "ORDER_STATUS_HELP" => await HandleOrderHelpAsync(conversation.Id, request, cancellationToken),
            "PRODUCT_DETAIL_HELP" => await HandleProductDetailHelpAsync(conversation.Id, request, cancellationToken),
            "PRODUCT_SEARCH" => await HandleProductSearchAsync(conversation.Id, request, cancellationToken),
            _ => BuildFallbackResponse(conversation.Id, request)
        };

        response = await RefineReplyWithLlmAsync(response, request, verifiedMemory, intentProfile, cancellationToken);
        response = AiChatPolicyGuard.Apply(response);

        await _messageRepository.AddAsync(
            AiChatMessage.Create(
                conversation.Id,
                AiChatSenderType.Assistant,
                response.Reply,
                response.Intent,
                SerializeOrNull(new
                {
                    response.Cards,
                    response.SuggestedReplies,
                    response.Recommendation,
                    response.SupportTicket,
                    intentProfile,
                    verifiedMemory,
                    policyGuardApplied = true
                })),
            cancellationToken);

        await TrackLearningEventAsync(conversation.Id, request, response, intentProfile, verifiedMemory, cancellationToken);

        return response;
    }

    /// <summary>
    /// Uses Gemini only to rewrite a grounded template reply. Business decisions and data lookup
    /// stay in the deterministic tool handlers above.
    /// </summary>
    private async Task<AiChatResponse> RefineReplyWithLlmAsync(
        AiChatResponse response,
        SendAiChatMessageCommand request,
        VerifiedConversationMemory verifiedMemory,
        AiChatIntentProfile intentProfile,
        CancellationToken cancellationToken)
    {
        var recentMessages = await _messageRepository.FindAsync(
            message => message.ConversationId == request.ConversationId,
            cancellationToken);
        var recentTurns = recentMessages
            .OrderByDescending(message => message.CreatedAt)
            .Take(8)
            .OrderBy(message => message.CreatedAt)
            .Select(message => new
            {
                sender = message.SenderType.ToString(),
                content = message.Content,
                intent = message.Intent
            })
            .ToList();

        var groundedFacts = SerializeOrNull(new
        {
            templateReply = response.Reply,
            intent = response.Intent,
            intentProfile,
            verifiedMemory,
            recentTurns,
            cards = response.Cards,
            recommendation = response.Recommendation,
            supportTicketCreated = response.SupportTicket is not null
        });

        var userPrompt = $"Tin nhắn mới nhất của khách: \"{request.Message}\"\n\n" +
            $"Dữ liệu đã được hệ thống kiểm tra sẵn (JSON, chỉ được dùng đúng dữ liệu này):\n{groundedFacts}";

        var refinedReply = await _geminiChatClient.GenerateReplyAsync(
            ReplyRefinementSystemInstruction, userPrompt, cancellationToken);

        return string.IsNullOrWhiteSpace(refinedReply) ? response : response with { Reply = refinedReply };
    }

    private async Task<AiChatResponse> HandleProductSearchAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var search = BuildProductSearch(request.Message);
        var toolInput = SerializeOrNull(search);

        IReadOnlyList<CatalogProductResponse> products;
        try
        {
            products = await _javaCoreApiClient.SearchPublicProductsAsync(search.Keyword, cancellationToken);
            if (products.Count == 0)
            {
                products = await _javaCoreApiClient.SearchPublicProductsAsync(null, cancellationToken);
            }

            await _toolRunRepository.AddAsync(
                AiChatToolRun.Succeeded(
                    conversationId,
                    "SearchProducts",
                    toolInput,
                    SerializeOrNull(new { count = products.Count })),
                cancellationToken);
        }
        catch (Exception exception) when (exception is ServiceUnavailableException or BadGatewayException)
        {
            await _toolRunRepository.AddAsync(
                AiChatToolRun.Failed(conversationId, "SearchProducts", toolInput, exception.Message),
                cancellationToken);
            throw;
        }

        var candidates = await BuildProductCandidatesAsync(products, cancellationToken);
        var filteredProducts = ApplySimpleFilters(candidates, search)
            .Take(5)
            .ToList();

        if (filteredProducts.Count == 0)
        {
            return new AiChatResponse(
                conversationId,
                "Em chưa tìm thấy sản phẩm thật sự khớp với yêu cầu này. Anh/chị muốn em thử tìm rộng hơn theo tên sản phẩm hoặc bỏ bớt điều kiện giá/màu không ạ?",
                "PRODUCT_SEARCH",
                Array.Empty<AiChatProductCard>(),
                new[] { "Tìm rộng hơn", "Bỏ điều kiện giá", "Tìm sản phẩm đang có sẵn" });
        }

        var cards = filteredProducts
            .Select(candidate => new AiChatProductCard(
                "PRODUCT",
                candidate.Product.Id,
                candidate.Product.Name,
                candidate.Product.BasePrice,
                candidate.Product.ThumbnailUrl,
                $"/products/{candidate.Product.Id}",
                candidate.AvailableSizes,
                candidate.AvailableColors))
            .ToList();

        var reply = cards.Count == 1
            ? "Em tìm được 1 sản phẩm rất hợp với yêu cầu của anh/chị, mẫu này đang còn hàng nên anh/chị chốt sớm để không lỡ size mình cần nhé. Anh/chị cho em xin chiều cao cân nặng, em tư vấn size chuẩn để mình đặt luôn ạ."
            : $"Em tìm được {cards.Count} sản phẩm rất hợp với yêu cầu của anh/chị, toàn mẫu đang có sẵn hàng ạ. Anh/chị ưng mẫu nào em tư vấn size ngay để mình chốt đơn luôn cho kịp còn hàng nhé.";

        return new AiChatResponse(
            conversationId,
            reply,
            "PRODUCT_SEARCH",
            cards,
            new[] { "Tư vấn size giúp tôi", "Mẫu nào đáng mua nhất?", "Tìm mẫu rẻ hơn" });
    }

    private async Task<AiChatResponse> HandleCartStatusAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId is null)
        {
            return BuildLoginRequiredCartResponse(conversationId);
        }

        try
        {
            var cart = await _javaCoreApiClient.GetCartAsync(request.UserId.Value, cancellationToken);
            await TrackCartToolRunAsync(conversationId, "GetCart", new { request.UserId }, cart, cancellationToken);
            return BuildCartStatusResponse(conversationId, cart);
        }
        catch (Exception exception) when (exception is AppValidationException
            or ConflictException
            or NotFoundException
            or ForbiddenException)
        {
            await _toolRunRepository.AddAsync(
                AiChatToolRun.Failed(conversationId, "GetCart", SerializeOrNull(new { request.UserId }), exception.Message),
                cancellationToken);

            return new AiChatResponse(
                conversationId,
                "Em chưa đọc được giỏ hàng hiện tại. Anh/chị thử đăng nhập lại hoặc mở trang giỏ hàng, nếu vẫn lỗi em sẽ tạo yêu cầu hỗ trợ cho mình.",
                "CART_STATUS",
                Array.Empty<AiChatProductCard>(),
                new[] { "Mở giỏ hàng", "Tạo yêu cầu hỗ trợ", "Tìm sản phẩm khác" });
        }
    }

    private async Task<AiChatResponse> HandleAddToCartAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId is null)
        {
            return BuildLoginRequiredCartResponse(conversationId);
        }

        var productId = request.Page?.ProductId;
        var productVariantId = request.ClientContext?.SelectedProductVariantId;
        var designId = request.ClientContext?.DesignId;
        var quantity = Math.Max(request.ClientContext?.Quantity ?? 1, 1);

        if (productId is null || productVariantId is null || designId is null)
        {
            var missing = new List<string>();
            if (productId is null)
            {
                missing.Add("sản phẩm");
            }

            if (productVariantId is null)
            {
                missing.Add("size/màu");
            }

            if (designId is null)
            {
                missing.Add("design đã lưu");
            }

            return new AiChatResponse(
                conversationId,
                $"Em chưa thêm vào giỏ được vì còn thiếu {string.Join(", ", missing)}. Anh/chị chọn đủ size/màu và lưu design trước, sau đó em thêm vào giỏ ngay cho mình.",
                "ADD_TO_CART",
                Array.Empty<AiChatProductCard>(),
                new[] { "Chọn size", "Lưu design", "Tư vấn size giúp tôi" });
        }

        var addRequest = new AddCartItemRequest(productId.Value, productVariantId.Value, designId.Value, quantity);
        try
        {
            var cart = await _javaCoreApiClient.AddCartItemAsync(request.UserId.Value, addRequest, cancellationToken);
            await TrackCartToolRunAsync(conversationId, "AddCartItem", addRequest, cart, cancellationToken);

            var total = cart.TotalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
            return new AiChatResponse(
                conversationId,
                $"Em đã thêm sản phẩm vào giỏ cho anh/chị rồi ạ. Giỏ hiện có {cart.TotalQuantity} sản phẩm, tạm tính {total}đ; mình có thể kiểm tra lại giỏ hoặc tiến tới checkout luôn.",
                "ADD_TO_CART",
                BuildCartProductCards(cart),
                new[] { "Xem giỏ hàng", "Checkout giỏ hàng", "Tìm thêm sản phẩm" });
        }
        catch (Exception exception) when (exception is AppValidationException
            or ConflictException
            or NotFoundException
            or ForbiddenException)
        {
            await _toolRunRepository.AddAsync(
                AiChatToolRun.Failed(conversationId, "AddCartItem", SerializeOrNull(addRequest), exception.Message),
                cancellationToken);

            return new AiChatResponse(
                conversationId,
                $"Em chưa thêm vào giỏ được: {exception.Message}. Anh/chị kiểm tra lại size/màu, tồn kho hoặc design đã lưu giúp em nhé.",
                "ADD_TO_CART",
                Array.Empty<AiChatProductCard>(),
                new[] { "Chọn size khác", "Lưu lại design", "Tạo yêu cầu hỗ trợ" });
        }
    }

    private async Task<AiChatResponse> HandleCartCheckoutAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId is null)
        {
            return BuildLoginRequiredCartResponse(conversationId);
        }

        if (string.IsNullOrWhiteSpace(request.ClientContext?.ReceiverName)
            || string.IsNullOrWhiteSpace(request.ClientContext.ReceiverPhone)
            || string.IsNullOrWhiteSpace(request.ClientContext.ShippingAddress))
        {
            return new AiChatResponse(
                conversationId,
                "Để checkout giỏ hàng, anh/chị cần nhập tên người nhận, số điện thoại và địa chỉ giao hàng trước. Khi form checkout có đủ thông tin, em sẽ tạo order từ giỏ cho mình.",
                "CART_CHECKOUT",
                Array.Empty<AiChatProductCard>(),
                new[] { "Nhập thông tin giao hàng", "Xem lại giỏ hàng", "Tạo yêu cầu hỗ trợ" });
        }

        var checkoutRequest = new CheckoutCartRequest(
            request.ClientContext.ReceiverName.Trim(),
            request.ClientContext.ReceiverPhone.Trim(),
            request.ClientContext.ShippingAddress.Trim());

        try
        {
            var order = await _javaCoreApiClient.CheckoutCartAsync(request.UserId.Value, checkoutRequest, cancellationToken);
            await _toolRunRepository.AddAsync(
                AiChatToolRun.Succeeded(
                    conversationId,
                    "CheckoutCart",
                    SerializeOrNull(checkoutRequest),
                    SerializeOrNull(new
                    {
                        order.OrderId,
                        order.OrderCode,
                        order.TotalAmount,
                        order.PaymentStatus,
                        order.OrderStatus
                    })),
                cancellationToken);

            var total = order.TotalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
            return new AiChatResponse(
                conversationId,
                $"Em đã tạo đơn {order.OrderCode} từ giỏ hàng, tổng tiền {total}đ. Bước tiếp theo anh/chị mở thanh toán để tạo PayOS QR/link và hoàn tất đơn nhé.",
                "CART_CHECKOUT",
                Array.Empty<AiChatProductCard>(),
                new[] { "Thanh toán đơn này", "Kiểm tra trạng thái đơn", "Tạo yêu cầu hỗ trợ" });
        }
        catch (Exception exception) when (exception is AppValidationException
            or ConflictException
            or NotFoundException
            or ForbiddenException)
        {
            await _toolRunRepository.AddAsync(
                AiChatToolRun.Failed(conversationId, "CheckoutCart", SerializeOrNull(checkoutRequest), exception.Message),
                cancellationToken);

            return new AiChatResponse(
                conversationId,
                $"Em chưa checkout giỏ hàng được: {exception.Message}. Anh/chị kiểm tra lại giỏ, tồn kho và thông tin giao hàng giúp em nhé.",
                "CART_CHECKOUT",
                Array.Empty<AiChatProductCard>(),
                new[] { "Xem lại giỏ hàng", "Tạo yêu cầu hỗ trợ", "Chọn sản phẩm khác" });
        }
    }

    private async Task<AiChatResponse> HandleSizeAdviceAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var height = ExtractCentimeters(request.Message);
        var weight = ExtractKilograms(request.Message);
        var fitPreference = ExtractFitPreference(request.Message);
        var product = await GetCurrentProductAsync(request.Page?.ProductId, cancellationToken);

        var availableVariants = GetAvailableVariants(product, request.ClientContext?.SelectedColor).ToList();
        var availableSizes = availableVariants
            .Select(variant => variant.Size)
            .Where(size => !string.IsNullOrWhiteSpace(size))
            .Select(size => size!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(SizeRank)
            .ToList();
        var availableColors = availableVariants
            .Select(variant => variant.Color)
            .Where(color => !string.IsNullOrWhiteSpace(color))
            .Select(color => color!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();

        if (height is null || weight is null)
        {
            var productHint = product is null
                ? " Nếu anh/chị đang xem một sản phẩm cụ thể, mở trang chi tiết sản phẩm sẽ giúp em kiểm tra size còn hàng chính xác hơn."
                : string.Empty;
            var sizeHint = availableSizes.Count > 0
                ? $" Mẫu này hiện còn các size: {string.Join(", ", availableSizes)}."
                : string.Empty;

            return new AiChatResponse(
                conversationId,
                $"Để tư vấn size sát hơn, anh/chị cho em xin chiều cao và cân nặng nhé. Ví dụ: em cao 1m65 nặng 55kg.{sizeHint}{productHint}",
                "SIZE_ADVICE",
                Array.Empty<AiChatProductCard>(),
                new[] { "Tôi cao 1m65 nặng 55kg", "Tôi thích mặc rộng", "Tôi thích mặc vừa người" });
        }

        var preferredSize = RecommendSize(height.Value, weight.Value, fitPreference);
        var size = ChooseAvailableSize(preferredSize, availableSizes);
        var availabilityText = availableSizes.Count > 0
            ? $" Các size đang còn: {string.Join(", ", availableSizes)}."
            : " Hiện em chưa đọc được bảng size/tồn kho chi tiết của mẫu này.";
        var colorText = availableColors.Count > 0
            ? $" Màu đang có: {string.Join(", ", availableColors)}."
            : string.Empty;
        var fitText = fitPreference switch
        {
            "LOOSE" => " theo kiểu mặc rộng thoải mái",
            "FITTED" => " theo kiểu mặc vừa/sát người",
            _ => string.Empty
        };
        var productText = product is null ? "mẫu này" : product.Name;
        var closingText = availableSizes.Count > 0
            ? $" Size {size} đang còn hàng, anh/chị chốt luôn size này để em không lo hết hàng giữa chừng nhé!"
            : " Anh/chị ưng thì mình chốt sớm size này nhé, em hỗ trợ thêm vào giỏ ngay ạ!";
        var reply = $"Với chiều cao khoảng {height}cm và cân nặng {weight}kg, em đề xuất size {size} cho {productText}{fitText} — mặc lên chắc chắn tôn dáng ạ. Nếu anh/chị thích mặc rộng hơn nữa thì nên ưu tiên size lớn hơn khi còn hàng.{availabilityText}{colorText}{closingText}";

        return new AiChatResponse(
            conversationId,
            reply,
            "SIZE_ADVICE",
            Array.Empty<AiChatProductCard>(),
            new[] { "So sánh với size lớn hơn", "Gợi ý phối đồ", "Thêm vào giỏ" },
            new AiChatSizeRecommendation(size, availableSizes.Count > 0 ? 0.78 : 0.66, "Dựa trên chiều cao/cân nặng và các size còn hàng của sản phẩm."));
    }

    private async Task<AiChatResponse> HandleProductDetailHelpAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var product = await GetCurrentProductAsync(request.Page?.ProductId, cancellationToken);
        var message = NormalizeForIntent(request.Message);

        if (product is null)
        {
            return new AiChatResponse(
                conversationId,
                "Em có thể tư vấn size, chất liệu, màu còn hàng hoặc cách phối đồ. Anh/chị mở trang chi tiết sản phẩm hoặc nói rõ mẫu đang quan tâm để em tư vấn sát hơn nhé.",
                "PRODUCT_DETAIL_HELP",
                Array.Empty<AiChatProductCard>(),
                new[] { "Tư vấn size", "Gợi ý phối đồ", "Tìm sản phẩm tương tự" });
        }

        var variants = GetAvailableVariants(product, request.ClientContext?.SelectedColor).ToList();
        var colors = variants
            .Select(variant => variant.Color)
            .Where(color => !string.IsNullOrWhiteSpace(color))
            .Select(color => color!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();
        var materials = variants
            .Select(variant => variant.Material)
            .Where(material => !string.IsNullOrWhiteSpace(material))
            .Select(material => material!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();

        string reply;
        if (ContainsAnyNormalized(message, "mau", "color"))
        {
            reply = colors.Count > 0
                ? $"Mẫu {product.Name} hiện còn các màu: {string.Join(", ", colors)} — màu nào lên đồ cũng đẹp ạ. Anh/chị chốt màu nào em tư vấn size luôn để mình đặt sớm kẻo hết màu đẹp nhé."
                : $"Em chưa đọc được màu còn hàng của mẫu {product.Name}. Anh/chị chọn biến thể trên trang sản phẩm giúp em, thấy màu ưng là mình chốt luôn cho chắc suất nhé.";
        }
        else if (ContainsAnyNormalized(message, "chat lieu", "vai", "material"))
        {
            reply = materials.Count > 0
                ? $"Mẫu {product.Name} có chất liệu/phiên bản: {string.Join(", ", materials)} — mặc lên rất đáng tiền ạ. Anh/chị ưu tiên thoáng mát hay đứng form, em tư vấn đúng phiên bản để mình chốt đơn luôn nhé."
                : $"Em chưa thấy dữ liệu chất liệu riêng cho mẫu {product.Name}, nhưng đây là mẫu đang được quan tâm ạ. Anh/chị để em tư vấn size/màu trước, ưng là mình đặt luôn nhé.";
        }
        else if (ContainsAnyNormalized(message, "phoi", "mac voi", "di lam", "di tiec", "di choi", "style"))
        {
            reply = $"Với mẫu {product.Name}, anh/chị phối màu trung tính với quần jean/quần tây là ăn điểm ngay, còn màu nổi đi cùng item trơn sẽ rất nổi bật. Anh/chị nói em dịp mặc và màu đang chọn, em lên luôn combo hoàn chỉnh để mình chốt trọn bộ nhé.";
        }
        else
        {
            reply = $"Anh/chị đang xem mẫu {product.Name} — lựa chọn rất đáng cân nhắc ạ. Em tư vấn size, màu còn hàng, chất liệu hay phối đồ đều được, anh/chị ưng phần nào em chốt giúp mình luôn nhé.";
        }

        return new AiChatResponse(
            conversationId,
            reply,
            "PRODUCT_DETAIL_HELP",
            Array.Empty<AiChatProductCard>(),
            new[] { "Tư vấn size", "Có màu nào khác không?", "Gợi ý phối đồ" });
    }

    private async Task<AiChatResponse> HandleOrderHelpAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId is null)
        {
            return new AiChatResponse(
                conversationId,
                "Để kiểm tra đơn hàng hoặc hỗ trợ lỗi xử lý order, anh/chị cần đăng nhập trước để em bảo vệ thông tin đơn của mình nhé.",
                "ORDER_STATUS_HELP",
                Array.Empty<AiChatProductCard>(),
                new[] { "Đăng nhập", "Tôi gặp lỗi xử lý đơn", "Liên hệ nhân viên" });
        }

        var orderId = request.Page?.OrderId ?? ExtractGuid(request.Message);
        if (orderId is null)
        {
            return new AiChatResponse(
                conversationId,
                $"Anh/chị mở đúng trang chi tiết đơn hàng hoặc gửi mã orderId để em kiểm tra trạng thái đơn và thanh toán nhé. {OrderProcessingIssuePrompt}",
                "ORDER_STATUS_HELP",
                Array.Empty<AiChatProductCard>(),
                new[] { "Tôi đang ở trang chi tiết đơn", "Đơn bị kẹt xử lý", "Đã thanh toán nhưng chưa cập nhật" });
        }

        OrderDetailResponse order;
        try
        {
            order = await _javaCoreApiClient.GetOrderDetailAsync(
                orderId.Value,
                request.UserId.Value,
                request.UserRole,
                cancellationToken);

            await _toolRunRepository.AddAsync(
                AiChatToolRun.Succeeded(
                    conversationId,
                    "GetOrderDetail",
                    SerializeOrNull(new { orderId }),
                    SerializeOrNull(new
                    {
                        order.OrderCode,
                        order.OrderStatus,
                        order.PaymentStatus
                    })),
                cancellationToken);
        }
        catch (NotFoundException)
        {
            return new AiChatResponse(
                conversationId,
                "Em chưa tìm thấy đơn hàng này hoặc đơn không thuộc tài khoản hiện tại. Anh/chị kiểm tra lại tài khoản đang đăng nhập giúp em nhé. Nếu đây là lỗi xử lý order, anh/chị có thể tạo yêu cầu hỗ trợ để nhân viên kiểm tra thêm.",
                "ORDER_STATUS_HELP",
                Array.Empty<AiChatProductCard>(),
                new[] { "Kiểm tra đơn khác", "Tạo yêu cầu hỗ trợ", "Tôi gặp lỗi xử lý đơn" });
        }
        catch (ForbiddenException)
        {
            return new AiChatResponse(
                conversationId,
                "Em không thể hiển thị thông tin đơn này vì đơn không thuộc quyền truy cập hiện tại. Nếu anh/chị đang gặp vấn đề xử lý order, mình quay lại đúng tài khoản đặt hàng rồi em kiểm tra tiếp nhé.",
                "ORDER_STATUS_HELP",
                Array.Empty<AiChatProductCard>(),
                new[] { "Về danh sách đơn hàng", "Tạo yêu cầu hỗ trợ", "Kiểm tra đơn khác" });
        }

        var payment = IsStaffOrAdmin(request.UserRole)
            ? await _paymentOrderRepository.GetByOrderIdAsync(order.Id, cancellationToken)
            : await _paymentOrderRepository.GetByOrderIdAndUserIdAsync(order.Id, request.UserId.Value, cancellationToken);

        await _toolRunRepository.AddAsync(
            AiChatToolRun.Succeeded(
                conversationId,
                "GetPaymentByOrder",
                SerializeOrNull(new { orderId = order.Id }),
                SerializeOrNull(payment is null
                    ? new { found = false }
                    : new
                    {
                        found = true,
                        payment.OrderCode,
                        Status = payment.Status.ToString().ToUpperInvariant(),
                        payment.Amount,
                        payment.PaidAt
                    })),
            cancellationToken);

        AiChatSupportTicketResponse? supportTicket = null;
        var wantsTicket = WantsSupportTicket(request.Message);
        if (wantsTicket)
        {
            var ticket = AiChatSupportTicket.Create(
                conversationId,
                request.UserId,
                order.Id,
                "ORDER_SUPPORT",
                $"User requested support for order {order.OrderCode}. Order={order.OrderStatus}, payment={order.PaymentStatus}. Message: {request.Message.Trim()}");

            await _supportTicketRepository.AddAsync(ticket, cancellationToken);

            supportTicket = new AiChatSupportTicketResponse(
                ticket.Id,
                ticket.IssueType,
                ticket.Severity,
                ticket.Status,
                ticket.Summary,
                ticket.CreatedAt);
        }

        var reply = BuildOrderStatusReply(order, payment, supportTicket is not null);

        return new AiChatResponse(
            conversationId,
            reply,
            "ORDER_STATUS_HELP",
            Array.Empty<AiChatProductCard>(),
            supportTicket is null
                ? new[] { "Đơn bị kẹt xử lý", "Đã thanh toán nhưng chưa cập nhật", "Tạo yêu cầu hỗ trợ" }
                : new[] { "Về danh sách đơn hàng", "Tiếp tục chat với nhân viên", "Kiểm tra đơn khác" },
            SupportTicket: supportTicket);
    }

    private static AiChatResponse BuildCheckoutHelpResponse(Guid conversationId)
    {
        return new AiChatResponse(
            conversationId,
            $"Nếu payment link đã được tạo, anh/chị có thể mở checkout URL hoặc quét QR PayOS để thanh toán. Anh/chị có đang gặp vấn đề khi xử lý order như thanh toán xong nhưng đơn chưa cập nhật, link lỗi, hoặc đơn bị kẹt không ạ? {OrderProcessingIssuePrompt}",
            "CHECKOUT_HELP",
            Array.Empty<AiChatProductCard>(),
            new[] { "Đã thanh toán nhưng chưa cập nhật", "Link thanh toán bị lỗi", "Kiểm tra đơn hàng" });
    }

    private static AiChatResponse BuildFallbackResponse(Guid conversationId, SendAiChatMessageCommand request)
    {
        var pageType = NormalizePageType(request.Page?.Type);
        var reply = pageType switch
        {
            "PRODUCT_DETAIL" => "Mẫu anh/chị đang xem rất đáng để rước về ạ. Em tư vấn size, chất liệu, màu còn hàng hoặc cách phối để anh/chị chốt được ngay, mình bắt đầu từ phần nào ạ?",
            "ORDER_DETAIL" => $"Em có thể kiểm tra trạng thái đơn hàng, thanh toán và xem đơn có bị kẹt xử lý không. Anh/chị có đang gặp vấn đề gì với order này không ạ? {OrderProcessingIssuePrompt}",
            "CHECKOUT" => $"Em có thể hướng dẫn thanh toán PayOS, kiểm tra link thanh toán hoặc hỗ trợ nếu anh/chị đã trả tiền nhưng đơn chưa cập nhật. Anh/chị có đang gặp vấn đề khi xử lý order không ạ?",
            _ => "Anh/chị mô tả rõ hơn nhu cầu giúp em nhé — em sẽ tìm đúng mẫu ưng ý, tư vấn size chuẩn và hỗ trợ chốt đơn nhanh gọn cho mình. Bên em cũng hỗ trợ phối đồ và theo dõi đơn hàng luôn ạ."
        };

        return new AiChatResponse(
            conversationId,
            reply,
            "GENERAL_HELP",
            Array.Empty<AiChatProductCard>(),
            pageType is "ORDER_DETAIL" or "CHECKOUT"
                ? new[] { "Đơn bị kẹt xử lý", "Đã thanh toán nhưng chưa cập nhật", "Tạo yêu cầu hỗ trợ" }
                : new[] { "Tìm sản phẩm", "Tư vấn size", "Hỗ trợ đơn hàng" });
    }

    private async Task<VerifiedConversationMemory> BuildVerifiedMemoryAsync(
        SendAiChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var recentMessages = await _messageRepository.FindAsync(
            message => message.ConversationId == request.ConversationId,
            cancellationToken);

        var recentIntentCounts = recentMessages
            .Where(message => !string.IsNullOrWhiteSpace(message.Intent))
            .GroupBy(message => message.Intent!)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var latestProductInterest = request.Page?.ProductId;
        var latestOrderContext = request.Page?.OrderId ?? ExtractGuid(request.Message);
        var behavior = BuildBehaviorContext(request, recentMessages);

        return new VerifiedConversationMemory(
            PageType: NormalizePageType(request.Page?.Type),
            PageUrl: request.Page?.Url,
            ProductId: latestProductInterest,
            OrderId: latestOrderContext,
            SelectedSize: NormalizeSize(request.ClientContext?.SelectedSize),
            SelectedColor: NormalizeColor(request.ClientContext?.SelectedColor),
            SelectedProductVariantId: request.ClientContext?.SelectedProductVariantId,
            DesignId: request.ClientContext?.DesignId,
            Quantity: request.ClientContext?.Quantity,
            HasCheckoutShippingInfo: !string.IsNullOrWhiteSpace(request.ClientContext?.ReceiverName)
                && !string.IsNullOrWhiteSpace(request.ClientContext?.ReceiverPhone)
                && !string.IsNullOrWhiteSpace(request.ClientContext?.ShippingAddress),
            CurrentFilters: request.ClientContext?.CurrentFilters,
            VisibleProductIds: request.ClientContext?.VisibleProductIds,
            HeightCm: ExtractCentimeters(request.Message),
            WeightKg: ExtractKilograms(request.Message),
            FitPreference: ExtractFitPreference(request.Message),
            RecentIntentCounts: recentIntentCounts,
            Behavior: behavior,
            Sources: new[] { "PAGE_CONTEXT", "CLIENT_CONTEXT", "RECENT_CHAT", "MESSAGE_EXTRACTION" });
    }

    private async Task TrackLearningEventAsync(
        Guid conversationId,
        SendAiChatMessageCommand request,
        AiChatResponse response,
        AiChatIntentProfile intentProfile,
        VerifiedConversationMemory verifiedMemory,
        CancellationToken cancellationToken)
    {
        var eventPayload = new
        {
            eventType = "AI_CHAT_TURN_OBSERVED",
            intentProfile,
            response.Intent,
            hasCards = response.Cards.Count > 0,
            hasRecommendation = response.Recommendation is not null,
            supportTicketCreated = response.SupportTicket is not null,
            suggestedReplyCount = response.SuggestedReplies.Count,
            userAuthenticated = request.UserId is not null,
            pageType = verifiedMemory.PageType,
            behavior = verifiedMemory.Behavior,
            observedAt = DateTime.UtcNow
        };

        await _toolRunRepository.AddAsync(
            AiChatToolRun.Succeeded(
                conversationId,
                "ConversationLearningEvent",
                SerializeOrNull(new { request.ConversationId, request.UserId }),
                SerializeOrNull(eventPayload)),
            cancellationToken);
    }

    private static AiChatBehaviorContext BuildBehaviorContext(
        SendAiChatMessageCommand request,
        IEnumerable<AiChatMessage> recentMessages)
    {
        var normalized = NormalizeForIntent(request.Message);
        var pageType = NormalizePageType(request.Page?.Type);
        var repeatedSupportSignals = recentMessages.Count(message =>
            string.Equals(message.Intent, "ORDER_STATUS_HELP", StringComparison.OrdinalIgnoreCase)
            || ContainsAnyNormalized(NormalizeForIntent(message.Content), "gap loi", "khieu nai", "ho tro", "nhan vien"));

        var buyingStage = pageType switch
        {
            "CHECKOUT" => "CHECKOUT",
            "ORDER_DETAIL" => "POST_PURCHASE",
            "PRODUCT_DETAIL" when request.ClientContext?.SelectedSize is not null => "DECISION",
            "PRODUCT_DETAIL" => "CONSIDERATION",
            "PRODUCT_LIST" => "DISCOVERY",
            _ => ContainsAnyNormalized(normalized, "mua", "chot", "thanh toan", "them vao gio") ? "DECISION" : "DISCOVERY"
        };

        var urgency = ContainsAnyNormalized(
            normalized,
            "ngay",
            "gap",
            "nhanh",
            "hom nay",
            "da thanh toan roi",
            "don bi ket")
            ? "HIGH"
            : "NORMAL";

        var objection = ContainsAnyNormalized(normalized, "mac", "dat", "re hon", "giam gia")
            ? "PRICE"
            : ContainsAnyNormalized(normalized, "so khong vua", "khong vua", "size")
                ? "FIT"
                : ContainsAnyNormalized(normalized, "loi", "khieu nai", "chua cap nhat", "ket")
                    ? "TRUST"
                    : null;

        var handoffRecommended = repeatedSupportSignals >= 2
            || ContainsAnyNormalized(normalized, "nhan vien", "khieu nai", "hoan tien", "gap quan ly");

        return new AiChatBehaviorContext(buyingStage, urgency, objection, handoffRecommended);
    }

    private static AiChatIntentProfile DetectIntentProfile(SendAiChatMessageCommand request)
    {
        var pageType = NormalizePageType(request.Page?.Type);
        var message = NormalizeForIntent(request.Message);
        var scores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["ADD_TO_CART"] = 0,
            ["CART_STATUS"] = 0,
            ["CART_CHECKOUT"] = 0,
            ["CHECKOUT_HELP"] = 0,
            ["SIZE_ADVICE"] = 0,
            ["ORDER_STATUS_HELP"] = 0,
            ["PRODUCT_DETAIL_HELP"] = 0,
            ["PRODUCT_SEARCH"] = 0,
            ["GENERAL_HELP"] = 0.1
        };
        var signals = new List<string>();

        if (pageType == "CART" || ContainsAnyNormalized(message, "gio hang", "cart", "trong gio"))
        {
            scores["CART_STATUS"] += pageType == "CART" ? 0.75 : 0.55;
            signals.Add("cart_context");
        }

        if (ContainsAnyNormalized(message, "them vao gio", "add to cart", "bo vao gio", "cho vao gio", "chot mau nay", "lay mau nay"))
        {
            scores["ADD_TO_CART"] += pageType == "PRODUCT_DETAIL" ? 0.85 : 0.7;
            signals.Add("add_to_cart_request");
        }

        if (ContainsAnyNormalized(message, "checkout gio", "checkout cart", "thanh toan gio", "dat hang tu gio", "chot gio hang"))
        {
            scores["CART_CHECKOUT"] += 0.9;
            signals.Add("cart_checkout_request");
        }

        if (pageType == "CHECKOUT" || ContainsAnyNormalized(message, "checkout", "qr", "payos", "link thanh toan", "loi thanh toan", "link loi"))
        {
            scores["CHECKOUT_HELP"] += pageType == "CHECKOUT" ? 0.75 : 0.55;
            signals.Add("checkout_or_payment_link");
        }

        if (ContainsAnyNormalized(message, "size", "cao", "nang", "kg", "mac vua", "mac rong", "chon co", "chon size"))
        {
            scores["SIZE_ADVICE"] += 0.7;
            signals.Add("size_or_body_measurement");
        }

        if (pageType == "ORDER_DETAIL"
            || ContainsAnyNormalized(
                message,
                "don hang",
                "ma don",
                "trang thai don",
                "order",
                "pending",
                "kiem tra thanh toan",
                "xu ly don",
                "don bi ket",
                "don chua cap nhat",
                "chua cap nhat",
                "da thanh toan roi",
                "gap loi xu ly",
                "loi xu ly",
                "van de voi don"))
        {
            scores["ORDER_STATUS_HELP"] += pageType == "ORDER_DETAIL" ? 0.8 : 0.65;
            signals.Add("order_or_support_issue");
        }

        if (pageType == "PRODUCT_DETAIL"
            || ContainsAnyNormalized(message, "phoi", "chat lieu", "vai", "co mau", "mau nao", "form ao", "style"))
        {
            scores["PRODUCT_DETAIL_HELP"] += pageType == "PRODUCT_DETAIL" ? 0.65 : 0.5;
            signals.Add("product_detail_context");
        }

        if (pageType == "PRODUCT_LIST"
            || ContainsAnyNormalized(message, "tim", "vay", "ao", "quan", "san pham", "mau", "duoi", "gia"))
        {
            scores["PRODUCT_SEARCH"] += pageType == "PRODUCT_LIST" ? 0.7 : 0.5;
            signals.Add("product_discovery");
        }

        if (scores["ORDER_STATUS_HELP"] > 0 && scores["CHECKOUT_HELP"] > 0)
        {
            scores["ORDER_STATUS_HELP"] += 0.15;
            signals.Add("payment_issue_needs_order_context");
        }

        var selected = scores
            .OrderByDescending(score => score.Value)
            .ThenBy(score => score.Key)
            .First();

        return new AiChatIntentProfile(
            selected.Key,
            Math.Min(selected.Value, 0.98),
            signals.Count == 0 ? new[] { "fallback" } : signals.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            RequiresTool: selected.Key is "ADD_TO_CART" or "CART_STATUS" or "CART_CHECKOUT" or "PRODUCT_SEARCH" or "PRODUCT_DETAIL_HELP" or "SIZE_ADVICE" or "ORDER_STATUS_HELP",
            HandoffRecommended: selected.Key == "ORDER_STATUS_HELP" && WantsSupportTicket(request.Message));
    }

    private static ProductSearch BuildProductSearch(string message)
    {
        return new ProductSearch(
            CleanKeyword(message),
            ExtractMaxPrice(message),
            ExtractColor(message));
    }

    private async Task<ProductDetailResponse?> GetCurrentProductAsync(Guid? productId, CancellationToken cancellationToken)
    {
        if (productId is null)
        {
            return null;
        }

        try
        {
            return await _javaCoreApiClient.GetPublicProductDetailAsync(productId.Value, cancellationToken);
        }
        catch (Exception exception) when (exception is NotFoundException
            or ForbiddenException
            or BadGatewayException
            or ServiceUnavailableException)
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<ProductCandidate>> BuildProductCandidatesAsync(
        IEnumerable<CatalogProductResponse> products,
        CancellationToken cancellationToken)
    {
        var candidates = new List<ProductCandidate>();

        foreach (var product in products)
        {
            ProductDetailResponse? detail = null;
            try
            {
                detail = await _javaCoreApiClient.GetPublicProductDetailAsync(product.Id, cancellationToken);
            }
            catch (Exception exception) when (exception is NotFoundException
                or ForbiddenException
                or BadGatewayException
                or ServiceUnavailableException)
            {
                // Product list may contain data that is no longer public, or Java may not
                // be able to provide variant details while the public product card is usable.
            }

            var availableVariants = GetAvailableVariants(detail, selectedColor: null).ToList();
            var sizes = availableVariants
                .Select(variant => variant.Size)
                .Where(size => !string.IsNullOrWhiteSpace(size))
                .Select(size => size!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(SizeRank)
                .ToList();
            var colors = availableVariants
                .Select(variant => variant.Color)
                .Where(color => !string.IsNullOrWhiteSpace(color))
                .Select(color => color!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order()
                .ToList();

            candidates.Add(new ProductCandidate(product, detail, sizes, colors));
        }

        return candidates;
    }

    private static IEnumerable<ProductCandidate> ApplySimpleFilters(
        IEnumerable<ProductCandidate> products,
        ProductSearch search)
    {
        var query = products;

        if (search.MaxPrice.HasValue)
        {
            query = query.Where(product => product.Product.BasePrice <= search.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.Color))
        {
            query = query.Where(product =>
                ContainsIgnoreCase(product.Product.Name, search.Color)
                || ContainsIgnoreCase(product.Product.Description, search.Color)
                || product.AvailableColors.Any(color => ContainsIgnoreCase(color, search.Color)));
        }

        return query;
    }

    private static decimal? ExtractMaxPrice(string message)
    {
        var match = PriceRegex().Match(message.ToLowerInvariant());
        if (!match.Success || !decimal.TryParse(match.Groups["value"].Value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return null;
        }

        var suffix = NormalizeForIntent(match.Groups["suffix"].Value);
        return suffix is "k" or "nghin" or "ngan" ? value * 1000 : value;
    }

    private static string? ExtractColor(string message)
    {
        var lower = NormalizeForIntent(message);
        var colors = new Dictionary<string, string>
        {
            ["trang"] = "white",
            ["white"] = "white",
            ["den"] = "black",
            ["black"] = "black",
            ["do"] = "red",
            ["red"] = "red",
            ["xanh"] = "blue",
            ["blue"] = "blue",
            ["hong"] = "pink",
            ["pink"] = "pink"
        };

        return colors.FirstOrDefault(color => lower.Contains(color.Key, StringComparison.OrdinalIgnoreCase)).Value;
    }

    private static string CleanKeyword(string message)
    {
        var keyword = PriceRegex().Replace(message, string.Empty);
        keyword = Regex.Replace(
            keyword,
            "\\b(tìm|tim|dưới|duoi|tầm|tam|khoảng|khoang|giá|gia|sản phẩm|san pham|cho tôi|giúp tôi|giup toi)\\b",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        keyword = Regex.Replace(keyword, "\\s+", " ").Trim(' ', ',', '.', '-', ':', ';');
        return string.IsNullOrWhiteSpace(keyword) ? message.Trim() : keyword.Trim();
    }

    private static int? ExtractCentimeters(string message)
    {
        var normalized = NormalizeForIntent(message);

        var meterMatch = Regex.Match(normalized, "(?<m>[1-2])\\s*m\\s*(?<cm>\\d{1,2})");
        if (meterMatch.Success && int.TryParse(meterMatch.Groups["cm"].Value, out var centimeters))
        {
            if (meterMatch.Groups["cm"].Value.Length == 1)
            {
                centimeters *= 10;
            }

            return (int.Parse(meterMatch.Groups["m"].Value, CultureInfo.InvariantCulture) * 100) + centimeters;
        }

        var decimalMeterMatch = Regex.Match(normalized, "(?<m>[1-2])[\\.,](?<cm>\\d{1,2})(?:\\s*m|\\b)");
        if (decimalMeterMatch.Success && int.TryParse(decimalMeterMatch.Groups["cm"].Value, out var decimalCentimeters))
        {
            if (decimalMeterMatch.Groups["cm"].Value.Length == 1)
            {
                decimalCentimeters *= 10;
            }

            return (int.Parse(decimalMeterMatch.Groups["m"].Value, CultureInfo.InvariantCulture) * 100) + decimalCentimeters;
        }

        var cmMatch = Regex.Match(normalized, "(?<cm>1\\d{2})\\s?cm");
        if (cmMatch.Success && int.TryParse(cmMatch.Groups["cm"].Value, out var cm))
        {
            return cm;
        }

        var plainCmAfterHeightWordMatch = Regex.Match(normalized, "(?:cao|chieu cao)\\s*(?<cm>1\\d{2})\\b");
        return plainCmAfterHeightWordMatch.Success && int.TryParse(plainCmAfterHeightWordMatch.Groups["cm"].Value, out var plainCm)
            ? plainCm
            : null;
    }

    private static int? ExtractKilograms(string message)
    {
        var normalized = NormalizeForIntent(message);
        var match = Regex.Match(normalized, "(?<kg>\\d{2,3})\\s*(?:kg|kgs|kilo|kilogram)");
        if (match.Success && int.TryParse(match.Groups["kg"].Value, out var kg))
        {
            return kg;
        }

        var weightWordMatch = Regex.Match(normalized, "(?:nang|can nang)\\s*(?<kg>\\d{2,3})\\b");
        return weightWordMatch.Success && int.TryParse(weightWordMatch.Groups["kg"].Value, out var plainKg)
            ? plainKg
            : null;
    }

    private static string? ExtractFitPreference(string message)
    {
        var normalized = NormalizeForIntent(message);
        if (ContainsAnyNormalized(normalized, "mac rong", "oversize", "oversized", "thoai mai", "form rong"))
        {
            return "LOOSE";
        }

        if (ContainsAnyNormalized(normalized, "mac vua", "vua nguoi", "om", "sat nguoi", "fit"))
        {
            return "FITTED";
        }

        return null;
    }

    private static bool ContainsAnyNormalized(string normalizedValue, params string[] normalizedTerms)
        => normalizedTerms.Any(term => normalizedValue.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static bool ContainsIgnoreCase(string? value, string term)
        => value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;

    private static string NormalizeForIntent(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character == 'đ' ? 'd' : character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool IsStaffOrAdmin(string? role)
        => string.Equals(role, "STAFF", StringComparison.OrdinalIgnoreCase)
           || string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

    private static bool WantsSupportTicket(string message)
    {
        var normalized = NormalizeForIntent(message);
        return ContainsAnyNormalized(
            normalized,
            "tao yeu cau",
            "yeu cau ho tro",
            "nhan vien",
            "support",
            "ticket",
            "khieu nai",
            "lien he",
            "co van de",
            "gap van de",
            "gap loi",
            "loi xu ly",
            "xu ly don",
            "don bi ket",
            "don chua cap nhat",
            "chua cap nhat",
            "da thanh toan roi",
            "thanh toan roi");
    }

    private async Task TrackCartToolRunAsync(
        Guid conversationId,
        string toolName,
        object? input,
        CartResponse cart,
        CancellationToken cancellationToken)
    {
        await _toolRunRepository.AddAsync(
            AiChatToolRun.Succeeded(
                conversationId,
                toolName,
                SerializeOrNull(input),
                SerializeOrNull(new
                {
                    cart.Id,
                    cart.TotalQuantity,
                    cart.TotalAmount,
                    itemCount = cart.Items.Count
                })),
            cancellationToken);
    }

    private static AiChatResponse BuildLoginRequiredCartResponse(Guid conversationId)
    {
        return new AiChatResponse(
            conversationId,
            "Để xem giỏ hàng, thêm sản phẩm vào giỏ hoặc checkout, anh/chị cần đăng nhập trước để em bảo vệ thông tin mua hàng của mình nhé.",
            "CART_STATUS",
            Array.Empty<AiChatProductCard>(),
            new[] { "Đăng nhập", "Tìm sản phẩm", "Tư vấn size" });
    }

    private static AiChatResponse BuildCartStatusResponse(Guid conversationId, CartResponse cart)
    {
        if (cart.Items.Count == 0)
        {
            return new AiChatResponse(
                conversationId,
                "Giỏ hàng của anh/chị hiện đang trống. Anh/chị nói em kiểu áo/quần muốn tìm, em gợi ý mẫu hợp rồi mình thêm vào giỏ luôn nhé.",
                "CART_STATUS",
                Array.Empty<AiChatProductCard>(),
                new[] { "Tìm áo thun", "Tìm sản phẩm đang có sẵn", "Tư vấn size" });
        }

        var total = cart.TotalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        var firstItems = cart.Items.Take(3).Select(item => $"{item.ProductName} x{item.Quantity}");
        return new AiChatResponse(
            conversationId,
            $"Giỏ hàng của anh/chị có {cart.TotalQuantity} sản phẩm, tạm tính {total}đ. Một vài món trong giỏ: {string.Join(", ", firstItems)}. Nếu thông tin giao hàng đã đủ, mình checkout luôn được ạ.",
            "CART_STATUS",
            BuildCartProductCards(cart),
            new[] { "Checkout giỏ hàng", "Tìm thêm sản phẩm", "Tạo yêu cầu hỗ trợ" });
    }

    private static IReadOnlyCollection<AiChatProductCard> BuildCartProductCards(CartResponse cart)
    {
        return cart.Items
            .Take(5)
            .Select(item => new AiChatProductCard(
                "PRODUCT",
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.PreviewImageUrl,
                $"/products/{item.ProductId}",
                string.IsNullOrWhiteSpace(item.Size) ? Array.Empty<string>() : new[] { item.Size },
                string.IsNullOrWhiteSpace(item.Color) ? Array.Empty<string>() : new[] { item.Color }))
            .ToList();
    }

    private static Guid? ExtractGuid(string message)
    {
        var match = GuidRegex().Match(message);
        return match.Success && Guid.TryParse(match.Value, out var value) ? value : null;
    }

    private static string BuildOrderStatusReply(
        OrderDetailResponse order,
        PaymentOrder? payment,
        bool ticketCreated)
    {
        var paymentStatus = payment?.Status.ToString().ToUpperInvariant() ?? order.PaymentStatus;
        var itemCount = order.Items?.Count ?? 0;
        var total = order.TotalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));

        var lines = new List<string>
        {
            $"Em đã kiểm tra đơn {order.OrderCode}.",
            $"Trạng thái đơn: {NormalizeStatusForCustomer(order.OrderStatus)}.",
            $"Trạng thái thanh toán: {NormalizeStatusForCustomer(paymentStatus)}.",
            $"Tổng tiền: {total}đ, gồm {itemCount} sản phẩm."
        };

        if (IsPendingStatus(order.PaymentStatus) || IsPendingStatus(paymentStatus))
        {
            lines.Add("Đơn này vẫn đang chờ xác nhận thanh toán. Nếu anh/chị đã thanh toán rồi, em khuyên tạo yêu cầu hỗ trợ để nhân viên kiểm tra PayOS/Kafka giúp.");
        }
        else if (IsPaidStatus(order.PaymentStatus) || IsPaidStatus(paymentStatus))
        {
            lines.Add("Thanh toán đã được ghi nhận. Nếu trạng thái đơn chưa chuyển bước tiếp theo, đơn có thể đang chờ nhân viên xử lý sản xuất.");
        }

        if (payment is null)
        {
            lines.Add("Em chưa thấy payment record bên .NET cho đơn này, nên nếu đơn có vấn đề thanh toán thì nên tạo yêu cầu hỗ trợ.");
        }

        if (ticketCreated)
        {
            lines.Add("Em đã tạo yêu cầu hỗ trợ cho đơn này. Nhân viên sẽ có thông tin order/payment để kiểm tra tiếp.");
        }
        else
        {
            lines.Add(OrderProcessingIssuePrompt);
        }

        return string.Join(" ", lines);
    }

    private static string NormalizeStatusForCustomer(string? status)
    {
        return status?.ToUpperInvariant() switch
        {
            "PENDING" => "đang chờ xử lý",
            "PAID" => "đã thanh toán",
            "CANCELLED" => "đã hủy",
            "CANCEL_REQUESTED" => "đang yêu cầu hủy",
            "EXPIRED" => "đã hết hạn",
            "IN_PRODUCTION" => "đang sản xuất",
            "SHIPPING" => "đang giao hàng",
            "COMPLETED" => "đã hoàn tất",
            null or "" => "chưa có dữ liệu",
            _ => status
        };
    }

    private static bool IsPendingStatus(string? status)
        => string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase);

    private static bool IsPaidStatus(string? status)
        => string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizePageType(string? pageType)
        => string.IsNullOrWhiteSpace(pageType) ? null : pageType.Trim().ToUpperInvariant();

    private static string? SerializeOrNull(object? value)
        => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);

    private static IEnumerable<ProductVariantResponse> GetAvailableVariants(ProductDetailResponse? product, string? selectedColor)
    {
        if (product?.Variants is null)
        {
            return Array.Empty<ProductVariantResponse>();
        }

        var normalizedColor = NormalizeColor(selectedColor);
        return product.Variants.Where(variant =>
            string.Equals(variant.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)
            && (variant.Inventory?.AvailableQuantity ?? 0) > 0
            && (normalizedColor is null || ContainsIgnoreCase(variant.Color, normalizedColor)));
    }

    private static string RecommendSize(int height, int weight, string? fitPreference)
    {
        var baseSize = weight switch
        {
            <= 48 => "S",
            <= 60 => "M",
            <= 72 => "L",
            _ => "XL"
        };

        if (height >= 180 && SizeRank(baseSize) < SizeRank("L"))
        {
            baseSize = "L";
        }

        if (string.Equals(fitPreference, "LOOSE", StringComparison.OrdinalIgnoreCase))
        {
            return IncreaseSize(baseSize);
        }

        return baseSize;
    }

    private static string IncreaseSize(string size)
        => NormalizeSize(size) switch
        {
            "XS" => "S",
            "S" => "M",
            "M" => "L",
            "L" => "XL",
            "XL" => "XXL",
            _ => size
        };

    private static string ChooseAvailableSize(string preferredSize, IReadOnlyCollection<string> availableSizes)
    {
        if (availableSizes.Count == 0)
        {
            return preferredSize;
        }

        if (availableSizes.Any(size => string.Equals(size, preferredSize, StringComparison.OrdinalIgnoreCase)))
        {
            return preferredSize;
        }

        return availableSizes
            .OrderBy(size => Math.Abs(SizeRank(size) - SizeRank(preferredSize)))
            .First();
    }

    private static int SizeRank(string? size)
    {
        return NormalizeSize(size) switch
        {
            "XS" => 0,
            "S" => 1,
            "M" => 2,
            "L" => 3,
            "XL" => 4,
            "XXL" => 5,
            "STANDARD" => 10,
            _ => 20
        };
    }

    private static string? NormalizeSize(string? size)
        => string.IsNullOrWhiteSpace(size) ? null : size.Trim().ToUpperInvariant();

    private static string? NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return null;
        }

        return ExtractColor(color) ?? color.Trim();
    }

    [GeneratedRegex("(?<value>\\d+(?:[\\.,]\\d+)?)\\s*(?<suffix>k|nghìn|ngàn|nghin|ngan|vnd|đ|₫)?", RegexOptions.IgnoreCase)]
    private static partial Regex PriceRegex();

    [GeneratedRegex("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
    private static partial Regex GuidRegex();

    private sealed record AiChatIntentProfile(
        string Intent,
        double Confidence,
        IReadOnlyCollection<string> Signals,
        bool RequiresTool,
        bool HandoffRecommended);

    private sealed record VerifiedConversationMemory(
        string? PageType,
        string? PageUrl,
        Guid? ProductId,
        Guid? OrderId,
        string? SelectedSize,
        string? SelectedColor,
        Guid? SelectedProductVariantId,
        Guid? DesignId,
        int? Quantity,
        bool HasCheckoutShippingInfo,
        IReadOnlyDictionary<string, string?>? CurrentFilters,
        IReadOnlyCollection<Guid>? VisibleProductIds,
        int? HeightCm,
        int? WeightKg,
        string? FitPreference,
        IReadOnlyDictionary<string, int> RecentIntentCounts,
        AiChatBehaviorContext Behavior,
        IReadOnlyCollection<string> Sources);

    private sealed record AiChatBehaviorContext(
        string BuyingStage,
        string Urgency,
        string? Objection,
        bool HandoffRecommended);

    private sealed record ProductSearch(string Keyword, decimal? MaxPrice, string? Color);

    private sealed record ProductCandidate(
        CatalogProductResponse Product,
        ProductDetailResponse? Detail,
        IReadOnlyCollection<string> AvailableSizes,
        IReadOnlyCollection<string> AvailableColors);
}

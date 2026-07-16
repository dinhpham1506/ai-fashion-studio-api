namespace AiFashionStudio.Platform.Application.Common.Dtos;

public record AiChatPageContext(
    string? Type,
    string? Url,
    Guid? ProductId,
    Guid? OrderId);

public record AiChatClientContext(
    IReadOnlyDictionary<string, string?>? CurrentFilters,
    IReadOnlyCollection<Guid>? VisibleProductIds,
    string? SelectedSize,
    string? SelectedColor,
    Guid? SelectedProductVariantId = null,
    Guid? DesignId = null,
    int? Quantity = null,
    string? ReceiverName = null,
    string? ReceiverPhone = null,
    string? ShippingAddress = null);

public record AiChatProductCard(
    string Type,
    Guid ProductId,
    string Name,
    decimal Price,
    string? ImageUrl,
    string NavigateUrl,
    IReadOnlyCollection<string> AvailableSizes,
    IReadOnlyCollection<string> AvailableColors);

public record AiChatSizeRecommendation(
    string Size,
    double Confidence,
    string Reason);

public record AiChatResponse(
    Guid ConversationId,
    string Reply,
    string Intent,
    IReadOnlyCollection<AiChatProductCard> Cards,
    IReadOnlyCollection<string> SuggestedReplies,
    AiChatSizeRecommendation? Recommendation = null,
    AiChatSupportTicketResponse? SupportTicket = null);

public record AiChatSupportTicketResponse(
    Guid Id,
    string IssueType,
    string Severity,
    string Status,
    string Summary,
    DateTime CreatedAt);

public record AiChatMessageResponse(
    Guid Id,
    string SenderType,
    string Content,
    string? Intent,
    DateTime CreatedAt);

public record AiChatConversationResponse(
    Guid ConversationId,
    string Status,
    string? PageType,
    Guid? RelatedProductId,
    Guid? RelatedOrderId,
    IReadOnlyCollection<AiChatMessageResponse> Messages);

public record CatalogProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,
    string? Status,
    string? ThumbnailUrl,
    DateTime? CreatedAt,
    DateTime? UpdatedAt);

public record ProductImageResponse(
    Guid Id,
    Guid ProductId,
    string ImageUrl,
    bool Thumbnail,
    int SortOrder,
    DateTime? CreatedAt);

public record ProductInventoryResponse(
    Guid VariantId,
    int AvailableQuantity);

public record ProductVariantResponse(
    Guid Id,
    Guid ProductId,
    string Sku,
    string? Size,
    string? Color,
    string? Material,
    decimal PriceAdjustment,
    string? Status,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    ProductInventoryResponse? Inventory);

public record ProductDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,
    string? Status,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<ProductImageResponse> Images,
    IReadOnlyCollection<ProductVariantResponse> Variants);

public record AddCartItemRequest(
    Guid ProductId,
    Guid ProductVariantId,
    Guid DesignId,
    int Quantity);

public record UpdateCartItemRequest(int Quantity);

public record CheckoutCartRequest(
    string ReceiverName,
    string ReceiverPhone,
    string ShippingAddress);

public record CartResponse(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<CartItemResponse> Items,
    int TotalQuantity,
    decimal TotalAmount);

public record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid ProductVariantId,
    string? Sku,
    string? Size,
    string? Color,
    string? Material,
    Guid DesignId,
    string? DesignName,
    string? PreviewImageUrl,
    int Quantity,
    int AvailableQuantity,
    decimal UnitPrice,
    decimal TotalPrice);

public record CartCheckoutResponse(
    Guid OrderId,
    string OrderCode,
    decimal TotalAmount,
    string PaymentStatus,
    string OrderStatus);

public record OrderStatusHistoryResponse(
    Guid Id,
    string? FromStatus,
    string ToStatus,
    Guid? ChangedBy,
    string? Note,
    DateTime CreatedAt);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    Guid? DesignId,
    string ProductNameSnapshot,
    IReadOnlyDictionary<string, object>? VariantSnapshot,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public record OrderDetailResponse(
    Guid Id,
    string OrderCode,
    Guid CustomerId,
    decimal TotalAmount,
    string PaymentStatus,
    string OrderStatus,
    string ReceiverName,
    string ReceiverPhone,
    string ShippingAddress,
    IReadOnlyCollection<OrderItemResponse> Items,
    IReadOnlyCollection<OrderStatusHistoryResponse> StatusHistory);

package com.aifashionstudio.ordering.api.dto;

import java.math.BigDecimal;
import java.util.UUID;

public record CartItemResponse(
        UUID id,
        UUID productId,
        String productName,
        UUID productVariantId,
        String sku,
        String size,
        String color,
        String material,
        UUID designId,
        String designName,
        String previewImageUrl,
        int quantity,
        int availableQuantity,
        BigDecimal unitPrice,
        BigDecimal totalPrice
) {
}

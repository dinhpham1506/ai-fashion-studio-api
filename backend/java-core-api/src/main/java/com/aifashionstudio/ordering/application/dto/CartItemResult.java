package com.aifashionstudio.ordering.application.dto;

import java.math.BigDecimal;
import java.util.UUID;

public record CartItemResult(
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

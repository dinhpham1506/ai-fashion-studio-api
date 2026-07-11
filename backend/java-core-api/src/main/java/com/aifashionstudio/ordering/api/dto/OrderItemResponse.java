package com.aifashionstudio.ordering.api.dto;

import java.math.BigDecimal;
import java.util.Map;
import java.util.UUID;

public record OrderItemResponse(
        UUID id,
        UUID productId,
        UUID productVariantId,
        UUID designId,
        String productNameSnapshot,
        Map<String, Object> variantSnapshot,
        int quantity,
        BigDecimal unitPrice,
        BigDecimal totalPrice
) {
}

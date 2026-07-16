package com.aifashionstudio.ordering.application.dto;

import java.math.BigDecimal;
import java.util.Map;
import java.util.UUID;

public record OrderItemResult(
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

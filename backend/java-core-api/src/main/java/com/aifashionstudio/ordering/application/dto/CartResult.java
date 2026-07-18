package com.aifashionstudio.ordering.application.dto;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

public record CartResult(
        UUID id,
        UUID customerId,
        List<CartItemResult> items,
        int totalQuantity,
        BigDecimal totalAmount
) {
}

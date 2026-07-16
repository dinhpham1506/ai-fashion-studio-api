package com.aifashionstudio.ordering.api.dto;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

public record CartResponse(
        UUID id,
        UUID customerId,
        List<CartItemResponse> items,
        int totalQuantity,
        BigDecimal totalAmount
) {
}

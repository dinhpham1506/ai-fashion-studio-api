package com.aifashionstudio.ordering.api.dto;

import java.math.BigDecimal;
import java.util.UUID;

public record OrderCreatedResponse(
        UUID orderId,
        String orderCode,
        BigDecimal totalAmount,
        String paymentStatus,
        String orderStatus
) {
}

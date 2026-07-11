package com.aifashionstudio.ordering.api.dto;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record OrderSummaryResponse(
        UUID id,
        String orderCode,
        BigDecimal totalAmount,
        String paymentStatus,
        String orderStatus,
        OffsetDateTime createdAt
) {
}

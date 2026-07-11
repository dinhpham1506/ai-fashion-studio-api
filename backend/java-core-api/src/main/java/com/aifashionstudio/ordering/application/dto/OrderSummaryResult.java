package com.aifashionstudio.ordering.application.dto;

import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.domain.model.PaymentStatus;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record OrderSummaryResult(
        UUID id,
        String orderCode,
        BigDecimal totalAmount,
        PaymentStatus paymentStatus,
        OrderStatus orderStatus,
        OffsetDateTime createdAt
) {
}

package com.aifashionstudio.ordering.application.dto;

import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.domain.model.PaymentStatus;

import java.math.BigDecimal;
import java.util.UUID;

public record OrderCreatedResult(
        UUID orderId,
        String orderCode,
        BigDecimal totalAmount,
        PaymentStatus paymentStatus,
        OrderStatus orderStatus
) {
}

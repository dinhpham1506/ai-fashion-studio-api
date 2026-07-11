package com.aifashionstudio.ordering.application.dto;

import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.domain.model.PaymentStatus;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

public record OrderDetailResult(
        UUID id,
        String orderCode,
        UUID customerId,
        BigDecimal totalAmount,
        PaymentStatus paymentStatus,
        OrderStatus orderStatus,
        String receiverName,
        String receiverPhone,
        String shippingAddress,
        List<OrderItemResult> items,
        List<OrderStatusHistoryResult> statusHistory
) {
}

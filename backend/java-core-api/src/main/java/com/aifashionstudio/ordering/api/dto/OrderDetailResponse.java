package com.aifashionstudio.ordering.api.dto;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

public record OrderDetailResponse(
        UUID id,
        String orderCode,
        UUID customerId,
        BigDecimal totalAmount,
        String paymentStatus,
        String orderStatus,
        String receiverName,
        String receiverPhone,
        String shippingAddress,
        List<OrderItemResponse> items,
        List<OrderStatusHistoryResponse> statusHistory
) {
}

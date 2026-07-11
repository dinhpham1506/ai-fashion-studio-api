package com.aifashionstudio.ordering.application.dto;

import com.aifashionstudio.ordering.domain.model.OrderStatus;

import java.util.UUID;

public record OrderStatusUpdatedResult(
        UUID orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus
) {
}

package com.aifashionstudio.ordering.application.command;

import com.aifashionstudio.ordering.domain.model.OrderStatus;

import java.util.UUID;

public record UpdateOrderStatusCommand(
        UUID staffId,
        UUID orderId,
        OrderStatus toStatus,
        String note
) {
}

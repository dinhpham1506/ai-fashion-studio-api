package com.aifashionstudio.ordering.api.dto;

import java.util.UUID;

public record OrderStatusUpdatedResponse(
        UUID orderId,
        String fromStatus,
        String toStatus
) {
}

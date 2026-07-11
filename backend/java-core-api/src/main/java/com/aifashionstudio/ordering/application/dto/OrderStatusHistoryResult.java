package com.aifashionstudio.ordering.application.dto;

import com.aifashionstudio.ordering.domain.model.OrderStatus;

import java.time.OffsetDateTime;
import java.util.UUID;

public record OrderStatusHistoryResult(
        UUID id,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        UUID changedBy,
        String note,
        OffsetDateTime createdAt
) {
}

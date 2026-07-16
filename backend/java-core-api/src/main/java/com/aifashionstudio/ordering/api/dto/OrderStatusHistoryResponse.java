package com.aifashionstudio.ordering.api.dto;

import java.time.OffsetDateTime;
import java.util.UUID;

public record OrderStatusHistoryResponse(
        UUID id,
        String fromStatus,
        String toStatus,
        UUID changedBy,
        String note,
        OffsetDateTime createdAt
) {
}

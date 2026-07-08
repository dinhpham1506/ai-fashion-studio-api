package com.aifashionstudio.catalog.api.dto;

import java.time.OffsetDateTime;
import java.util.UUID;

public record ProductImageResponse(
        UUID id,
        UUID productId,
        String imageUrl,
        boolean thumbnail,
        int sortOrder,
        OffsetDateTime createdAt
) {
}

package com.aifashionstudio.catalog.application.dto;

import java.time.OffsetDateTime;
import java.util.UUID;

public record ProductImageResult(
        UUID id,
        UUID productId,
        String imageUrl,
        boolean thumbnail,
        int sortOrder,
        OffsetDateTime createdAt
) {
}

package com.aifashionstudio.design.api.dto;

import java.time.OffsetDateTime;
import java.util.UUID;

public record DesignSummaryResponse(
        UUID id,
        String name,
        UUID productId,
        UUID productVariantId,
        String previewImageUrl,
        String status,
        OffsetDateTime createdAt
) {
}

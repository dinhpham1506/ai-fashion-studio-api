package com.aifashionstudio.design.application.dto;

import com.aifashionstudio.design.domain.model.DesignStatus;

import java.time.OffsetDateTime;
import java.util.UUID;

public record DesignSummaryResult(
        UUID id,
        String name,
        UUID productId,
        UUID productVariantId,
        String previewImageUrl,
        DesignStatus status,
        OffsetDateTime createdAt
) {
}

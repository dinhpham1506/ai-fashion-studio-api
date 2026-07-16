package com.aifashionstudio.catalog.application.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record CatalogResult(
        UUID id,
        String name,
        String description,
        BigDecimal basePrice,
        CatalogStatus status,
        String thumbnailUrl,
        OffsetDateTime createdAt,
        OffsetDateTime updatedAt
) {
}

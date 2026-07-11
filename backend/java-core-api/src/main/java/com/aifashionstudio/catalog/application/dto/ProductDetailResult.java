package com.aifashionstudio.catalog.application.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.List;
import java.util.UUID;

public record ProductDetailResult(
        UUID id,
        String name,
        String description,
        BigDecimal basePrice,
        CatalogStatus status,
        OffsetDateTime createdAt,
        OffsetDateTime updatedAt,
        List<ProductImageResult> images,
        List<ProductVariantResult> variants
) {
}

package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.List;
import java.util.UUID;

public record ProductDetailResponse(
        UUID id,
        String name,
        String description,
        BigDecimal basePrice,
        CatalogStatus status,
        OffsetDateTime createdAt,
        OffsetDateTime updatedAt,
        List<ProductImageResponse> images,
        List<ProductVariantResponse> variants
) {
}

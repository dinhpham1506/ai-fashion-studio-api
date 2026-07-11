package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record ProductVariantResponse(
        UUID id,
        UUID productId,
        String sku,
        String size,
        String color,
        String material,
        BigDecimal priceAdjustment,
        ProductVariantStatus status,
        OffsetDateTime createdAt,
        OffsetDateTime updatedAt,
        InventorySummaryResponse inventory
) {
}

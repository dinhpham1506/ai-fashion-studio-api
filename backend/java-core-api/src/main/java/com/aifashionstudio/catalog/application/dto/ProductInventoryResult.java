package com.aifashionstudio.catalog.application.dto;

import java.time.OffsetDateTime;
import java.util.UUID;

public record ProductInventoryResult(
        UUID id,
        UUID variantId,
        int availableQuantity,
        int reservedQuantity,
        int soldQuantity,
        OffsetDateTime updatedAt
) {
}

package com.aifashionstudio.catalog.api.dto;

import java.time.OffsetDateTime;
import java.util.UUID;

public record ProductInventoryResponse(
        UUID id,
        UUID variantId,
        int availableQuantity,
        int reservedQuantity,
        int soldQuantity,
        OffsetDateTime updatedAt
) {
}

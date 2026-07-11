package com.aifashionstudio.catalog.api.dto;

import java.util.UUID;

public record InventorySummaryResponse(
        UUID variantId,
        int availableQuantity
) {
}

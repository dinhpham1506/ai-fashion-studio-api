package com.aifashionstudio.catalog.api.dto;

import jakarta.validation.constraints.Min;

public record UpdateInventoryRequest(
        @Min(0)
        int availableQuantity
) {
}

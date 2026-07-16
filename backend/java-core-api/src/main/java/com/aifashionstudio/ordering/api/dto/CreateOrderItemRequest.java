package com.aifashionstudio.ordering.api.dto;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;

import java.util.UUID;

public record CreateOrderItemRequest(
        @NotNull
        UUID productId,

        @NotNull
        UUID productVariantId,

        UUID designId,

        @Min(1)
        int quantity
) {
}

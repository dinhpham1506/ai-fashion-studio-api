package com.aifashionstudio.ordering.api.dto;

import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;

import java.util.UUID;

public record AddCartItemRequest(
        @NotNull
        UUID productId,

        @NotNull
        UUID productVariantId,

        @NotNull
        UUID designId,

        @Positive
        int quantity
) {
}

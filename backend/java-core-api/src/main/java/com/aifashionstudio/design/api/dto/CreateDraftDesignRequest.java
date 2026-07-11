package com.aifashionstudio.design.api.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.util.UUID;

public record CreateDraftDesignRequest(
        @NotNull
        UUID productId,

        @NotNull
        UUID productVariantId,

        @NotBlank
        String name
) {
}

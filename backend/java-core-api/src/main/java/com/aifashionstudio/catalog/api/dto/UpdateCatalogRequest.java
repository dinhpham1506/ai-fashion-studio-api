package com.aifashionstudio.catalog.api.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.PositiveOrZero;

import java.math.BigDecimal;

public record UpdateCatalogRequest(
        @Schema(description = "Updated unique catalog product name", example = "Oversized Black Hoodie")
        @NotBlank String name,
        @Schema(description = "Updated catalog product description", example = "Warm hoodie optimized for custom printed designs")
        String description,
        @Schema(description = "Updated base product price before variant adjustments", example = "399000")
        @NotNull @PositiveOrZero BigDecimal basePrice
) {
}

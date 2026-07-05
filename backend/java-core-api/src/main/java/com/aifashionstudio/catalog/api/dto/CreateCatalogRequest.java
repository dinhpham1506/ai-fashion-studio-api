package com.aifashionstudio.catalog.api.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.PositiveOrZero;

import java.math.BigDecimal;

public record CreateCatalogRequest(
       @Schema(description = "Unique catalog product name", example = "Classic White T-Shirt")
       @NotBlank String name,
       @Schema(description = "Catalog product description", example = "Premium cotton T-shirt for custom AI designs")
       String description,
       @Schema(description = "Base product price before variant adjustments", example = "199000")
       @NotNull @PositiveOrZero BigDecimal basePrice
) {

}

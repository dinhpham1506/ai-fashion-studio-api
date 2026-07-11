package com.aifashionstudio.catalog.api.dto;

import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.math.BigDecimal;

public record UpdateProductVariantRequest(
        @NotBlank
        String sku,
        @NotBlank
        String size,
        @NotBlank
        String color,
        @NotBlank
        String material,
        @NotNull
        @DecimalMin("0.00")
        BigDecimal priceAdjustment
) {
}

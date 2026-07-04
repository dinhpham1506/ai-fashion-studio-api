package com.aifashionstudio.catalog.api.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.PositiveOrZero;

import java.math.BigDecimal;

public record CreateCatalogRequest(
       @NotBlank String name,
       String description,
       @NotNull @PositiveOrZero BigDecimal basePrice
) {

}

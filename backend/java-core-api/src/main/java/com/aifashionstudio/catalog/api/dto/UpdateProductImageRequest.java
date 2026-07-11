package com.aifashionstudio.catalog.api.dto;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;

public record UpdateProductImageRequest(
        @NotBlank
        String imageUrl,
        boolean thumbnail,
        @Min(0)
        int sortOrder
) {
}

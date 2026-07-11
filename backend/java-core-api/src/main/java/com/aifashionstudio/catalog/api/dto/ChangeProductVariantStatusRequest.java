package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import jakarta.validation.constraints.NotNull;

public record ChangeProductVariantStatusRequest(
        @NotNull
        ProductVariantStatus status
) {
}

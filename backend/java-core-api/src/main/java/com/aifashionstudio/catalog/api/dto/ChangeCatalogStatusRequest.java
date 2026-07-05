package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import jakarta.validation.constraints.NotNull;

public record ChangeCatalogStatusRequest(
        @NotNull CatalogStatus status
) {
}

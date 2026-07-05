package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotNull;

public record ChangeCatalogStatusRequest(
        @Schema(description = "New catalog product status", example = "ACTIVE")
        @NotNull CatalogStatus status
) {
}

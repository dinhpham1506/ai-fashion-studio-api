package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import io.swagger.v3.oas.annotations.media.Schema;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record CatalogResponse(
        @Schema(description = "Catalog product ID", example = "3fa85f64-5717-4562-b3fc-2c963f66afa6")
        UUID id,
        @Schema(description = "Catalog product name", example = "Classic White T-Shirt")
        String name,
        @Schema(description = "Catalog product description", example = "Premium cotton T-shirt for custom AI designs")
        String description,
        @Schema(description = "Base product price before variant adjustments", example = "199000")
        BigDecimal basePrice,
        @Schema(description = "Catalog product status", example = "ACTIVE")
        CatalogStatus status,
        @Schema(description = "Product thumbnail image URL")
        String thumbnailUrl,
        @Schema(description = "Creation timestamp")
        OffsetDateTime createdAt,
        @Schema(description = "Last update timestamp")
        OffsetDateTime updatedAt
        // offsetDatetime dùng khi cần lưu trữ thông tin thời gian có múi giờ,
        // ví dụ khi làm việc với các hệ thống phân tán hoặc
        // khi cần lưu trữ thời gian chính xác theo múi giờ của người dùng
        //
) {
}

package com.aifashionstudio.catalog.api.dto;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record CatalogResponse(
        UUID id,
        String name,
        String description,
        BigDecimal basePrice,
        CatalogStatus status,
        OffsetDateTime createdAt,
        OffsetDateTime updatedAt
        // offsetDatetime dùng khi cần lưu trữ thông tin thời gian có múi giờ,
        // ví dụ khi làm việc với các hệ thống phân tán hoặc
        // khi cần lưu trữ thời gian chính xác theo múi giờ của người dùng
        //
) {
}

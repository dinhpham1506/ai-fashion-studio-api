package com.aifashionstudio.design.application.dto;

import com.aifashionstudio.design.domain.model.DesignStatus;

import java.util.List;
import java.util.Map;
import java.util.UUID;

public record DesignDetailResult(
        UUID id,
        UUID customerId,
        UUID productId,
        UUID productVariantId,
        String name,
        Map<String, Object> canvasJson,
        String previewImageUrl,
        String printFileUrl,
        DesignStatus status,
        List<DesignLayerResult> layers
) {
}

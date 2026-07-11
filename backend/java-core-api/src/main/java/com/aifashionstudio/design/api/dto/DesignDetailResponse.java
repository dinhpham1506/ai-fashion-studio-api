package com.aifashionstudio.design.api.dto;

import java.util.List;
import java.util.Map;
import java.util.UUID;

public record DesignDetailResponse(
        UUID id,
        UUID customerId,
        UUID productId,
        UUID productVariantId,
        String name,
        Map<String, Object> canvasJson,
        String previewImageUrl,
        String printFileUrl,
        String status,
        List<DesignLayerResponse> layers
) {
}

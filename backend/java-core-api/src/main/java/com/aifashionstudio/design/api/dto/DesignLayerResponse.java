package com.aifashionstudio.design.api.dto;

import java.math.BigDecimal;
import java.util.UUID;

public record DesignLayerResponse(
        UUID id,
        String layerType,
        String content,
        BigDecimal positionX,
        BigDecimal positionY,
        BigDecimal width,
        BigDecimal height,
        BigDecimal rotation,
        String color,
        int zIndex
) {
}

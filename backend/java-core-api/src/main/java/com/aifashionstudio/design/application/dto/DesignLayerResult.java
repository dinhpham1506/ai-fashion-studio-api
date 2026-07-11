package com.aifashionstudio.design.application.dto;

import com.aifashionstudio.design.domain.model.DesignLayerType;

import java.math.BigDecimal;
import java.util.UUID;

public record DesignLayerResult(
        UUID id,
        DesignLayerType layerType,
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

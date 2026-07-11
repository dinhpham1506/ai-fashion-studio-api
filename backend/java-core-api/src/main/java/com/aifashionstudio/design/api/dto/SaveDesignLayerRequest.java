package com.aifashionstudio.design.api.dto;

import com.aifashionstudio.design.domain.model.DesignLayerType;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;

import java.math.BigDecimal;

public record SaveDesignLayerRequest(
        @NotNull
        DesignLayerType layerType,

        String content,

        @NotNull
        BigDecimal positionX,

        @NotNull
        BigDecimal positionY,

        @NotNull
        @Positive
        BigDecimal width,

        @NotNull
        @Positive
        BigDecimal height,

        BigDecimal rotation,

        String color,

        int zIndex
) {
}

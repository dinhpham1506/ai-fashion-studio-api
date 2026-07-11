package com.aifashionstudio.design.application.command;

import com.aifashionstudio.design.domain.model.DesignLayerType;

import java.math.BigDecimal;
import java.util.List;
import java.util.Map;
import java.util.UUID;

public record SaveDesignCommand(
        UUID customerId,
        UUID designId,
        String name,
        Map<String, Object> canvasJson,
        String previewImageUrl,
        String printFileUrl,
        List<SaveDesignLayerCommand> layers
) {
    public record SaveDesignLayerCommand(
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
}

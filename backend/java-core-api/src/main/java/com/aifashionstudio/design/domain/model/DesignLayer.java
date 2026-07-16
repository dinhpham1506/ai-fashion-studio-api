package com.aifashionstudio.design.domain.model;

import com.aifashionstudio.shared.domain.model.common.BaseDomainModel;

import java.math.BigDecimal;
import java.util.UUID;

public class DesignLayer extends BaseDomainModel {

    private UUID designId;
    private DesignLayerType layerType;
    private String content;
    private BigDecimal positionX;
    private BigDecimal positionY;
    private BigDecimal width;
    private BigDecimal height;
    private BigDecimal rotation;
    private String color;
    private int zIndex;

    protected DesignLayer() {
    }

    public static DesignLayer create(UUID designId,
                                     DesignLayerType layerType,
                                     String content,
                                     BigDecimal positionX,
                                     BigDecimal positionY,
                                     BigDecimal width,
                                     BigDecimal height,
                                     BigDecimal rotation,
                                     String color,
                                     int zIndex) {
        if (designId == null) {
            throw new IllegalArgumentException("Design id cannot be null");
        }
        if (layerType == null) {
            throw new IllegalArgumentException("Layer type cannot be null");
        }
        validateRequired(positionX, "Position X cannot be null");
        validateRequired(positionY, "Position Y cannot be null");
        validatePositive(width, "Width must be greater than zero");
        validatePositive(height, "Height must be greater than zero");

        DesignLayer layer = new DesignLayer();
        layer.designId = designId;
        layer.layerType = layerType;
        layer.content = normalize(content);
        layer.positionX = positionX;
        layer.positionY = positionY;
        layer.width = width;
        layer.height = height;
        layer.rotation = rotation == null ? BigDecimal.ZERO : rotation;
        layer.color = normalize(color);
        layer.zIndex = zIndex;
        return layer;
    }

    public static DesignLayer reconstitute(UUID id,
                                           UUID designId,
                                           DesignLayerType layerType,
                                           String content,
                                           BigDecimal positionX,
                                           BigDecimal positionY,
                                           BigDecimal width,
                                           BigDecimal height,
                                           BigDecimal rotation,
                                           String color,
                                           int zIndex) {
        DesignLayer layer = create(designId, layerType, content, positionX, positionY, width, height, rotation, color, zIndex);
        layer.setId(id);
        return layer;
    }

    public UUID getDesignId() {
        return designId;
    }

    public DesignLayerType getLayerType() {
        return layerType;
    }

    public String getContent() {
        return content;
    }

    public BigDecimal getPositionX() {
        return positionX;
    }

    public BigDecimal getPositionY() {
        return positionY;
    }

    public BigDecimal getWidth() {
        return width;
    }

    public BigDecimal getHeight() {
        return height;
    }

    public BigDecimal getRotation() {
        return rotation;
    }

    public String getColor() {
        return color;
    }

    public int getZIndex() {
        return zIndex;
    }

    private static void validateRequired(BigDecimal value, String message) {
        if (value == null) {
            throw new IllegalArgumentException(message);
        }
    }

    private static void validatePositive(BigDecimal value, String message) {
        validateRequired(value, message);
        if (value.compareTo(BigDecimal.ZERO) <= 0) {
            throw new IllegalArgumentException(message);
        }
    }

    private static String normalize(String value) {
        return value == null || value.isBlank() ? null : value.trim();
    }
}

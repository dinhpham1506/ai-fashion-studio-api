package com.aifashionstudio.design.infrastructure.persistence.mapper;

import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.model.DesignLayer;
import com.aifashionstudio.design.infrastructure.persistence.entity.DesignLayerJpaEntity;
import com.aifashionstudio.design.infrastructure.persistence.entity.DesignJpaEntity;
import org.springframework.stereotype.Component;

@Component
public class DesignPersistenceMapper {

    public Design toDomain(DesignJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return Design.reconstitute(
                entity.getId(),
                entity.getCustomerId(),
                entity.getProductId(),
                entity.getProductVariantId(),
                entity.getName(),
                entity.getCanvasJson(),
                entity.getPreviewImageUrl(),
                entity.getPrintFileUrl(),
                entity.getStatus(),
                entity.getCreatedAt(),
                entity.getUpdatedAt()
        );
    }

    public DesignJpaEntity toEntity(Design domain) {
        if (domain == null) {
            return null;
        }

        return DesignJpaEntity.builder()
                .id(domain.getId())
                .customerId(domain.getCustomerId())
                .productId(domain.getProductId())
                .productVariantId(domain.getProductVariantId())
                .name(domain.getName())
                .canvasJson(domain.getCanvasJson())
                .previewImageUrl(domain.getPreviewImageUrl())
                .printFileUrl(domain.getPrintFileUrl())
                .status(domain.getStatus())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .build();
    }

    public DesignLayer toDomain(DesignLayerJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return DesignLayer.reconstitute(
                entity.getId(),
                entity.getDesignId(),
                entity.getLayerType(),
                entity.getContent(),
                entity.getPositionX(),
                entity.getPositionY(),
                entity.getWidth(),
                entity.getHeight(),
                entity.getRotation(),
                entity.getColor(),
                entity.getZIndex()
        );
    }

    public DesignLayerJpaEntity toEntity(DesignLayer domain) {
        if (domain == null) {
            return null;
        }

        return DesignLayerJpaEntity.builder()
                .id(domain.getId())
                .designId(domain.getDesignId())
                .layerType(domain.getLayerType())
                .content(domain.getContent())
                .positionX(domain.getPositionX())
                .positionY(domain.getPositionY())
                .width(domain.getWidth())
                .height(domain.getHeight())
                .rotation(domain.getRotation())
                .color(domain.getColor())
                .zIndex(domain.getZIndex())
                .build();
    }
}

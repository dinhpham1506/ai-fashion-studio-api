package com.aifashionstudio.design.application.mapper;

import com.aifashionstudio.design.application.dto.DesignDetailResult;
import com.aifashionstudio.design.application.dto.DesignDraftResult;
import com.aifashionstudio.design.application.dto.DesignLayerResult;
import com.aifashionstudio.design.application.dto.DesignSavedResult;
import com.aifashionstudio.design.application.dto.DesignSummaryResult;
import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.model.DesignLayer;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class DesignApplicationMapper {

    public DesignDraftResult toDraftResult(Design design) {
        return new DesignDraftResult(
                design.getId(),
                design.getStatus()
        );
    }

    public DesignSavedResult toSavedResult(Design design) {
        return new DesignSavedResult(
                design.getId(),
                design.getStatus(),
                design.getPreviewImageUrl(),
                design.getPrintFileUrl()
        );
    }

    public DesignSummaryResult toSummaryResult(Design design) {
        return new DesignSummaryResult(
                design.getId(),
                design.getName(),
                design.getProductId(),
                design.getProductVariantId(),
                design.getPreviewImageUrl(),
                design.getStatus(),
                design.getCreatedAt()
        );
    }

    public DesignDetailResult toDetailResult(Design design, List<DesignLayer> layers) {
        return new DesignDetailResult(
                design.getId(),
                design.getCustomerId(),
                design.getProductId(),
                design.getProductVariantId(),
                design.getName(),
                design.getCanvasJson(),
                design.getPreviewImageUrl(),
                design.getPrintFileUrl(),
                design.getStatus(),
                layers.stream()
                        .map(this::toLayerResult)
                        .toList()
        );
    }

    private DesignLayerResult toLayerResult(DesignLayer layer) {
        return new DesignLayerResult(
                layer.getId(),
                layer.getLayerType(),
                layer.getContent(),
                layer.getPositionX(),
                layer.getPositionY(),
                layer.getWidth(),
                layer.getHeight(),
                layer.getRotation(),
                layer.getColor(),
                layer.getZIndex()
        );
    }
}

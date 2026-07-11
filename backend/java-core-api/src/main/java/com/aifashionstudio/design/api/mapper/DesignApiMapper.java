package com.aifashionstudio.design.api.mapper;

import com.aifashionstudio.design.api.dto.CreateDraftDesignRequest;
import com.aifashionstudio.design.api.dto.DesignDetailResponse;
import com.aifashionstudio.design.api.dto.DesignDraftResponse;
import com.aifashionstudio.design.api.dto.DesignLayerResponse;
import com.aifashionstudio.design.api.dto.DesignSavedResponse;
import com.aifashionstudio.design.api.dto.DesignSummaryResponse;
import com.aifashionstudio.design.api.dto.PagedDesignResponse;
import com.aifashionstudio.design.api.dto.SaveDesignRequest;
import com.aifashionstudio.design.application.command.CreateDraftDesignCommand;
import com.aifashionstudio.design.application.command.SaveDesignCommand;
import com.aifashionstudio.design.application.dto.DesignDetailResult;
import com.aifashionstudio.design.application.dto.DesignDraftResult;
import com.aifashionstudio.design.application.dto.DesignLayerResult;
import com.aifashionstudio.design.application.dto.DesignSavedResult;
import com.aifashionstudio.design.application.dto.DesignSummaryResult;
import com.aifashionstudio.design.application.dto.PagedDesignResult;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.UUID;

@Component
public class DesignApiMapper {

    public CreateDraftDesignCommand toCommand(UUID customerId, CreateDraftDesignRequest request) {
        return new CreateDraftDesignCommand(
                customerId,
                request.productId(),
                request.productVariantId(),
                request.name()
        );
    }

    public DesignDraftResponse toResponse(DesignDraftResult result) {
        return new DesignDraftResponse(
                result.designId(),
                result.status().name()
        );
    }

    public SaveDesignCommand toCommand(UUID customerId, UUID designId, SaveDesignRequest request) {
        return new SaveDesignCommand(
                customerId,
                designId,
                request.name(),
                request.canvasJson(),
                request.previewImageUrl(),
                request.printFileUrl(),
                request.layers() == null ? List.of() : request.layers().stream()
                        .map(layer -> new SaveDesignCommand.SaveDesignLayerCommand(
                                layer.layerType(),
                                layer.content(),
                                layer.positionX(),
                                layer.positionY(),
                                layer.width(),
                                layer.height(),
                                layer.rotation(),
                                layer.color(),
                                layer.zIndex()
                        ))
                        .toList()
        );
    }

    public DesignSavedResponse toResponse(DesignSavedResult result) {
        return new DesignSavedResponse(
                result.designId(),
                result.status().name(),
                result.previewImageUrl(),
                result.printFileUrl()
        );
    }

    public PagedDesignResponse toResponse(PagedDesignResult result) {
        return new PagedDesignResponse(
                result.items().stream()
                        .map(this::toResponse)
                        .toList(),
                result.page(),
                result.pageSize(),
                result.totalItems(),
                result.totalPages()
        );
    }

    private DesignSummaryResponse toResponse(DesignSummaryResult result) {
        return new DesignSummaryResponse(
                result.id(),
                result.name(),
                result.productId(),
                result.productVariantId(),
                result.previewImageUrl(),
                result.status().name(),
                result.createdAt()
        );
    }

    public DesignDetailResponse toResponse(DesignDetailResult result) {
        return new DesignDetailResponse(
                result.id(),
                result.customerId(),
                result.productId(),
                result.productVariantId(),
                result.name(),
                result.canvasJson(),
                result.previewImageUrl(),
                result.printFileUrl(),
                result.status().name(),
                result.layers().stream()
                        .map(this::toResponse)
                        .toList()
        );
    }

    private DesignLayerResponse toResponse(DesignLayerResult result) {
        return new DesignLayerResponse(
                result.id(),
                result.layerType().name(),
                result.content(),
                result.positionX(),
                result.positionY(),
                result.width(),
                result.height(),
                result.rotation(),
                result.color(),
                result.zIndex()
        );
    }
}

package com.aifashionstudio.design.application.service.impl;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.design.application.command.CreateDraftDesignCommand;
import com.aifashionstudio.design.application.command.SaveDesignCommand;
import com.aifashionstudio.design.application.dto.DesignDetailResult;
import com.aifashionstudio.design.application.dto.DesignDraftResult;
import com.aifashionstudio.design.application.dto.DesignSavedResult;
import com.aifashionstudio.design.application.dto.PagedDesignResult;
import com.aifashionstudio.design.application.mapper.DesignApplicationMapper;
import com.aifashionstudio.design.application.service.DesignApplicationService;
import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.model.DesignLayer;
import com.aifashionstudio.design.domain.model.DesignStatus;
import com.aifashionstudio.design.domain.repository.DesignLayerRepository;
import com.aifashionstudio.design.domain.repository.DesignRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ConflictException;
import com.aifashionstudio.shared.exception.ForbiddenException;
import com.aifashionstudio.shared.exception.NotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@RequiredArgsConstructor
public class DesignApplicationServiceImpl implements DesignApplicationService {
    private static final int MAX_LAYER_COUNT = 50;

    private final CatalogRepository catalogRepository;
    private final ProductVariantRepository productVariantRepository;
    private final DesignRepository designRepository;
    private final DesignLayerRepository designLayerRepository;
    private final DesignApplicationMapper mapper;

    @Override
    @Transactional
    public DesignDraftResult createDraft(CreateDraftDesignCommand command) {
        Catalog product = catalogRepository.findById(command.productId())
                .orElseThrow(() -> new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + command.productId()));

        if (product.getStatus() != CatalogStatus.ACTIVE) {
            throw new BusinessRuleException("PRODUCT_NOT_AVAILABLE", "Product is not available");
        }

        ProductVariant variant = productVariantRepository.findById(command.productVariantId())
                .orElseThrow(() -> new NotFoundException("VARIANT_NOT_FOUND", "Variant not found with id: " + command.productVariantId()));

        if (!variant.getProduct().getId().equals(product.getId())) {
            throw new BusinessRuleException("VARIANT_NOT_AVAILABLE", "Variant does not belong to product");
        }

        if (variant.getStatus() != ProductVariantStatus.ACTIVE) {
            throw new BusinessRuleException("VARIANT_NOT_AVAILABLE", "Variant is not available");
        }

        Design design = Design.createDraft(
                command.customerId(),
                command.productId(),
                command.productVariantId(),
                command.name()
        );

        return mapper.toDraftResult(designRepository.save(design));
    }

    @Override
    @Transactional
    public DesignSavedResult saveDesign(SaveDesignCommand command) {
        Design design = designRepository.findById(command.designId())
                .orElseThrow(() -> new NotFoundException("DESIGN_NOT_FOUND", "Design not found with id: " + command.designId()));

        if (!design.getCustomerId().equals(command.customerId())) {
            throw new ForbiddenException("DESIGN_ACCESS_DENIED", "Design access denied");
        }

        if (design.getStatus() == DesignStatus.LOCKED) {
            throw new ConflictException("DESIGN_LOCKED", "Design is locked");
        }

        List<SaveDesignCommand.SaveDesignLayerCommand> layerCommands = command.layers() == null ? List.of() : command.layers();
        if (layerCommands.size() > MAX_LAYER_COUNT) {
            throw new BusinessRuleException("DESIGN_LAYER_LIMIT_EXCEEDED", "Design layer limit exceeded");
        }

        try {
            design.save(command.name(), command.canvasJson(), command.previewImageUrl(), command.printFileUrl());
        } catch (IllegalArgumentException ex) {
            throw new BusinessRuleException("INVALID_CANVAS_JSON", ex.getMessage());
        } catch (IllegalStateException ex) {
            throw new ConflictException("DESIGN_LOCKED", ex.getMessage());
        }

        Design savedDesign = designRepository.save(design);
        designLayerRepository.deleteByDesignId(savedDesign.getId());
        designLayerRepository.saveAll(layerCommands.stream()
                .map(layer -> DesignLayer.create(
                        savedDesign.getId(),
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
                .toList());

        return mapper.toSavedResult(savedDesign);
    }

    @Override
    public PagedDesignResult getMyDesigns(java.util.UUID customerId, int page, int pageSize) {
        if (page < 1) {
            throw new BusinessRuleException("INVALID_PAGE", "Page must be greater than zero");
        }
        if (pageSize < 1 || pageSize > 100) {
            throw new BusinessRuleException("INVALID_PAGE_SIZE", "Page size must be between 1 and 100");
        }

        long totalItems = designRepository.countByCustomerId(customerId);
        int totalPages = totalItems == 0 ? 0 : (int) Math.ceil((double) totalItems / pageSize);

        return new PagedDesignResult(
                designRepository.findByCustomerId(customerId, page, pageSize)
                        .stream()
                        .map(mapper::toSummaryResult)
                        .toList(),
                page,
                pageSize,
                totalItems,
                totalPages
        );
    }

    @Override
    public DesignDetailResult getDesignDetail(java.util.UUID customerId, java.util.UUID designId) {
        Design design = designRepository.findById(designId)
                .orElseThrow(() -> new NotFoundException("DESIGN_NOT_FOUND", "Design not found with id: " + designId));

        if (!design.getCustomerId().equals(customerId)) {
            throw new ForbiddenException("DESIGN_ACCESS_DENIED", "Design access denied");
        }

        return mapper.toDetailResult(
                design,
                designLayerRepository.findByDesignIdOrderByZIndexAsc(design.getId())
        );
    }
}

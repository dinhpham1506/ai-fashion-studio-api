package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.UpdateInventoryCommand;
import com.aifashionstudio.catalog.application.dto.ProductInventoryResult;
import com.aifashionstudio.catalog.application.mapper.ProductCatalogApplicationMapper;
import com.aifashionstudio.catalog.application.service.ProductInventoryApplicationService;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.NotFoundException;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ProductInventoryApplicationServiceImpl implements ProductInventoryApplicationService {

    private final ProductVariantRepository productVariantRepository;
    private final ProductInventoryRepository productInventoryRepository;
    private final ProductCatalogApplicationMapper mapper;

    @Override
    public ProductInventoryResult getInventoryByVariantId(UUID variantId) {
        getVariantOrThrow(variantId);
        return getInventoryOrThrow(variantId);
    }

    @Override
    public ProductInventoryResult getPublicInventoryByVariantId(UUID variantId) {
        ProductVariant variant = getVariantOrThrow(variantId);
        if (variant.getStatus() != ProductVariantStatus.ACTIVE
                || variant.getProduct().getStatus() != CatalogStatus.ACTIVE) {
            throw variantNotFound(variantId);
        }

        return getInventoryOrThrow(variantId);
    }

    @Override
    @Transactional
    public ProductInventoryResult updateInventory(UUID variantId, UpdateInventoryCommand command) {
        if (command.availableQuantity() < 0) {
            throw new BusinessRuleException("INVALID_INVENTORY_QUANTITY", "Available quantity cannot be negative");
        }

        ProductVariant variant = getVariantOrThrow(variantId);
        ProductInventory inventory = productInventoryRepository.findByProductVariantId(variantId)
                .orElseGet(() -> {
                    ProductInventory created = new ProductInventory();
                    created.setProductVariant(variant);
                    created.setReservedQuantity(0);
                    created.setSoldQuantity(0);
                    return created;
                });

        inventory.setAvailableQuantity(command.availableQuantity());
        return mapper.toInventoryResult(productInventoryRepository.save(inventory));
    }

    private ProductInventoryResult getInventoryOrThrow(UUID variantId) {
        return productInventoryRepository.findByProductVariantId(variantId)
                .map(mapper::toInventoryResult)
                .orElseThrow(() -> new NotFoundException(
                        "PRODUCT_INVENTORY_NOT_FOUND",
                        "Product inventory not found for variant id: " + variantId
                ));
    }

    private ProductVariant getVariantOrThrow(UUID variantId) {
        return productVariantRepository.findById(variantId)
                .orElseThrow(() -> variantNotFound(variantId));
    }

    private NotFoundException variantNotFound(UUID variantId) {
        return new NotFoundException(
                "PRODUCT_VARIANT_NOT_FOUND",
                "Product variant not found with id: " + variantId
        );
    }
}

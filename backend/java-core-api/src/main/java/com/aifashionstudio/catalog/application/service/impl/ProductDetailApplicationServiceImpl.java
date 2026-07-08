package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.dto.ProductDetailResult;
import com.aifashionstudio.catalog.application.dto.ProductImageResult;
import com.aifashionstudio.catalog.application.dto.ProductInventoryResult;
import com.aifashionstudio.catalog.application.dto.ProductVariantResult;
import com.aifashionstudio.catalog.application.mapper.ProductCatalogApplicationMapper;
import com.aifashionstudio.catalog.application.service.ProductDetailApplicationService;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductImageRepository;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.shared.exception.NotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ProductDetailApplicationServiceImpl implements ProductDetailApplicationService {

    private final CatalogRepository catalogRepository;
    private final ProductImageRepository productImageRepository;
    private final ProductVariantRepository productVariantRepository;
    private final ProductInventoryRepository productInventoryRepository;
    private final ProductCatalogApplicationMapper mapper;

    @Override
    public ProductDetailResult getPublicProductDetail(UUID productId) {
        Catalog product = catalogRepository.findById(productId)
                .orElseThrow(() -> productNotFound(productId));

        if (product.getStatus() != CatalogStatus.ACTIVE) {
            throw productNotFound(productId);
        }

        List<ProductImageResult> images = productImageRepository.findByProductIdOrderBySortOrderAsc(productId)
                .stream()
                .map(mapper::toImageResult)
                .toList();

        List<ProductVariantResult> variants = productVariantRepository
                .findByProductIdAndStatus(productId, ProductVariantStatus.ACTIVE)
                .stream()
                .map(variant -> {
                    ProductInventoryResult inventory = productInventoryRepository.findByProductVariantId(variant.getId())
                            .map(mapper::toInventoryResult)
                            .orElseGet(() -> mapper.zeroInventoryResult(variant));
                    return mapper.toVariantResult(variant, inventory);
                })
                .toList();

        return mapper.toProductDetailResult(product, images, variants);
    }

    private NotFoundException productNotFound(UUID productId) {
        return new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + productId);
    }
}

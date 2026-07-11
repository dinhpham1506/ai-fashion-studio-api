package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.CreateProductImageCommand;
import com.aifashionstudio.catalog.application.command.UpdateProductImageCommand;
import com.aifashionstudio.catalog.application.dto.ProductImageResult;
import com.aifashionstudio.catalog.application.mapper.ProductCatalogApplicationMapper;
import com.aifashionstudio.catalog.application.service.ProductImageApplicationService;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductImage;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductImageRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.NotFoundException;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ProductImageApplicationServiceImpl implements ProductImageApplicationService {

    private final CatalogRepository catalogRepository;
    private final ProductImageRepository productImageRepository;
    private final ProductCatalogApplicationMapper mapper;

    @Override
    @Transactional
    public ProductImageResult addImage(UUID productId, CreateProductImageCommand command) {
        Catalog product = getCatalogOrThrow(productId);
        validateImage(command.imageUrl(), command.sortOrder());
        clearExistingThumbnail(productId, null, command.thumbnail());

        ProductImage image = new ProductImage();
        image.setProduct(product);
        image.setImageUrl(command.imageUrl().trim());
        image.setThumbnail(command.thumbnail());
        image.setSortOrder(command.sortOrder());

        return mapper.toImageResult(productImageRepository.save(image));
    }

    @Override
    @Transactional
    public ProductImageResult updateImage(UUID productId, UUID imageId, UpdateProductImageCommand command) {
        getCatalogOrThrow(productId);
        validateImage(command.imageUrl(), command.sortOrder());

        ProductImage image = getImageOrThrow(imageId);
        assertImageBelongsToProduct(productId, image);
        clearExistingThumbnail(productId, imageId, command.thumbnail());

        image.setImageUrl(command.imageUrl().trim());
        image.setThumbnail(command.thumbnail());
        image.setSortOrder(command.sortOrder());

        return mapper.toImageResult(productImageRepository.save(image));
    }

    @Override
    public List<ProductImageResult> getImagesByProductId(UUID productId) {
        getCatalogOrThrow(productId);
        return productImageRepository.findByProductIdOrderBySortOrderAsc(productId)
                .stream()
                .map(mapper::toImageResult)
                .toList();
    }

    @Override
    public List<ProductImageResult> getPublicImagesByProductId(UUID productId) {
        Catalog product = getCatalogOrThrow(productId);
        if (product.getStatus() != CatalogStatus.ACTIVE) {
            throw productNotFound(productId);
        }

        return productImageRepository.findByProductIdOrderBySortOrderAsc(productId)
                .stream()
                .map(mapper::toImageResult)
                .toList();
    }

    @Override
    @Transactional
    public void deleteImage(UUID productId, UUID imageId) {
        getCatalogOrThrow(productId);
        ProductImage image = getImageOrThrow(imageId);
        assertImageBelongsToProduct(productId, image);
        productImageRepository.delete(image);
    }

    private void clearExistingThumbnail(UUID productId, UUID imageId, boolean newImageIsThumbnail) {
        if (!newImageIsThumbnail) {
            return;
        }

        productImageRepository.findByProductIdAndThumbnailTrue(productId)
                .filter(existing -> !existing.getId().equals(imageId))
                .ifPresent(existing -> {
                    existing.setThumbnail(false);
                    productImageRepository.save(existing);
                });
    }

    private ProductImage getImageOrThrow(UUID imageId) {
        return productImageRepository.findById(imageId)
                .orElseThrow(() -> new NotFoundException(
                        "PRODUCT_IMAGE_NOT_FOUND",
                        "Product image not found with id: " + imageId
                ));
    }

    private Catalog getCatalogOrThrow(UUID productId) {
        return catalogRepository.findById(productId)
                .orElseThrow(() -> new NotFoundException(
                        "CATALOG_NOT_FOUND",
                        "Catalog not found with id: " + productId
                ));
    }

    private void assertImageBelongsToProduct(UUID productId, ProductImage image) {
        if (!image.getProduct().getId().equals(productId)) {
            throw new NotFoundException(
                    "PRODUCT_IMAGE_NOT_FOUND",
                    "Product image not found with id: " + image.getId()
            );
        }
    }

    private void validateImage(String imageUrl, int sortOrder) {
        if (imageUrl == null || imageUrl.isBlank()) {
            throw new BusinessRuleException("INVALID_PRODUCT_IMAGE", "Product image URL cannot be null or empty");
        }
        if (sortOrder < 0) {
            throw new BusinessRuleException("INVALID_PRODUCT_IMAGE", "Product image sort order cannot be negative");
        }
    }

    private NotFoundException productNotFound(UUID productId) {
        return new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + productId);
    }
}

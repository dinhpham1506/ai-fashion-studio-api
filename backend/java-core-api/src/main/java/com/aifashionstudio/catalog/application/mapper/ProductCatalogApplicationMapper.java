package com.aifashionstudio.catalog.application.mapper;

import com.aifashionstudio.catalog.application.dto.ProductDetailResult;
import com.aifashionstudio.catalog.application.dto.ProductImageResult;
import com.aifashionstudio.catalog.application.dto.ProductInventoryResult;
import com.aifashionstudio.catalog.application.dto.ProductVariantResult;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.ProductImage;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class ProductCatalogApplicationMapper {

    public ProductVariantResult toVariantResult(ProductVariant variant, ProductInventoryResult inventory) {
        return new ProductVariantResult(
                variant.getId(),
                variant.getProduct().getId(),
                variant.getSku(),
                variant.getSize(),
                variant.getColor(),
                variant.getMaterial(),
                variant.getPriceAdjustment(),
                variant.getStatus(),
                variant.getCreatedAt(),
                variant.getUpdatedAt(),
                inventory
        );
    }

    public ProductInventoryResult toInventoryResult(ProductInventory inventory) {
        return new ProductInventoryResult(
                inventory.getId(),
                inventory.getProductVariant().getId(),
                inventory.getAvailableQuantity(),
                inventory.getReservedQuantity(),
                inventory.getSoldQuantity(),
                inventory.getUpdatedAt()
        );
    }

    public ProductInventoryResult zeroInventoryResult(ProductVariant variant) {
        return new ProductInventoryResult(
                null,
                variant.getId(),
                0,
                0,
                0,
                null
        );
    }

    public ProductImageResult toImageResult(ProductImage image) {
        return new ProductImageResult(
                image.getId(),
                image.getProduct().getId(),
                image.getImageUrl(),
                image.isThumbnail(),
                image.getSortOrder(),
                image.getCreatedAt()
        );
    }

    public ProductDetailResult toProductDetailResult(
            Catalog product,
            List<ProductImageResult> images,
            List<ProductVariantResult> variants
    ) {
        return new ProductDetailResult(
                product.getId(),
                product.getName(),
                product.getDescription(),
                product.getBasePrice(),
                product.getStatus(),
                product.getCreatedAt(),
                product.getUpdatedAt(),
                images,
                variants
        );
    }
}

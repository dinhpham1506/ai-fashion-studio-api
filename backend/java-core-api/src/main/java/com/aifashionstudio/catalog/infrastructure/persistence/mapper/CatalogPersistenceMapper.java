package com.aifashionstudio.catalog.infrastructure.persistence.mapper;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.ProductImage;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.infrastructure.persistence.entity.CatalogJpaEntity;
import com.aifashionstudio.catalog.infrastructure.persistence.entity.ProductImageJpaEntity;
import com.aifashionstudio.catalog.infrastructure.persistence.entity.ProductInventoryJpaEntity;
import com.aifashionstudio.catalog.infrastructure.persistence.entity.ProductVariantJpaEntity;
import org.springframework.stereotype.Component;

@Component
public class CatalogPersistenceMapper {

    public Catalog toDomain(CatalogJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return Catalog.reconstitute(
                entity.getId(),
                entity.getName(),
                entity.getDescription(),
                entity.getBasePrice(),
                entity.getStatus(),
                entity.getCreatedBy(),
                entity.getCreatedAt(),
                entity.getUpdatedAt()
        );
    }

    public CatalogJpaEntity toEntity(Catalog domain) {
        if (domain == null) {
            return null;
        }

        return CatalogJpaEntity.builder()
                .id(domain.getId())
                .name(domain.getName())
                .description(domain.getDescription())
                .basePrice(domain.getBasePrice())
                .status(domain.getStatus())
                .createdBy(domain.getCreatedBy())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .build();
    }

    public ProductVariant toDomain(ProductVariantJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return ProductVariant.reconstitute(
                entity.getId(),
                toDomain(entity.getProduct()),
                entity.getSku(),
                entity.getSize(),
                entity.getColor(),
                entity.getMaterial(),
                entity.getPriceAdjustment(),
                entity.getStatus(),
                entity.getCreatedAt(),
                entity.getUpdatedAt()
        );
    }

    public ProductVariantJpaEntity toEntity(ProductVariant domain) {
        if (domain == null) {
            return null;
        }

        return ProductVariantJpaEntity.builder()
                .id(domain.getId())
                .product(catalogReference(domain.getProduct()))
                .sku(domain.getSku())
                .size(domain.getSize())
                .color(domain.getColor())
                .material(domain.getMaterial())
                .priceAdjustment(domain.getPriceAdjustment())
                .status(domain.getStatus())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .build();
    }

    public ProductImage toDomain(ProductImageJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return ProductImage.reconstitute(
                entity.getId(),
                toDomain(entity.getProduct()),
                entity.getImageUrl(),
                entity.isThumbnail(),
                entity.getSortOrder(),
                entity.getCreatedAt()
        );
    }

    public ProductImageJpaEntity toEntity(ProductImage domain) {
        if (domain == null) {
            return null;
        }

        return ProductImageJpaEntity.builder()
                .id(domain.getId())
                .product(catalogReference(domain.getProduct()))
                .imageUrl(domain.getImageUrl())
                .thumbnail(domain.isThumbnail())
                .sortOrder(domain.getSortOrder())
                .createdAt(domain.getCreatedAt())
                .build();
    }

    public ProductInventory toDomain(ProductInventoryJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return ProductInventory.reconstitute(
                entity.getId(),
                toDomain(entity.getProductVariant()),
                entity.getAvailableQuantity(),
                entity.getReservedQuantity(),
                entity.getSoldQuantity(),
                entity.getUpdatedAt()
        );
    }

    public ProductInventoryJpaEntity toEntity(ProductInventory domain) {
        if (domain == null) {
            return null;
        }

        return ProductInventoryJpaEntity.builder()
                .id(domain.getId())
                .productVariant(productVariantReference(domain.getProductVariant()))
                .availableQuantity(domain.getAvailableQuantity())
                .reservedQuantity(domain.getReservedQuantity())
                .soldQuantity(domain.getSoldQuantity())
                .updatedAt(domain.getUpdatedAt())
                .build();
    }

    private CatalogJpaEntity catalogReference(Catalog domain) {
        if (domain == null || domain.getId() == null) {
            return null;
        }

        return CatalogJpaEntity.builder()
                .id(domain.getId())
                .build();
    }

    private ProductVariantJpaEntity productVariantReference(ProductVariant domain) {
        if (domain == null || domain.getId() == null) {
            return null;
        }

        return ProductVariantJpaEntity.builder()
                .id(domain.getId())
                .build();
    }
}

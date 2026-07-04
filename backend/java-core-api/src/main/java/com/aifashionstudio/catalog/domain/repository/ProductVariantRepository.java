package com.aifashionstudio.catalog.domain.repository;

import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface ProductVariantRepository {

    ProductVariant save(ProductVariant productVariant);

    Optional<ProductVariant> findById(UUID id);

    List<ProductVariant> findByProductId(UUID productId);

    List<ProductVariant> findByProductIdAndStatus(UUID productId, ProductVariantStatus status);

    Optional<ProductVariant> findBySku(String sku);

    boolean existsBySku(String sku);
}

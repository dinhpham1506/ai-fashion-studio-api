package com.aifashionstudio.catalog.domain.repository;

import com.aifashionstudio.catalog.domain.model.ProductInventory;

import java.util.Optional;
import java.util.UUID;

public interface ProductInventoryRepository {

    ProductInventory save(ProductInventory productInventory);

    Optional<ProductInventory> findByProductVariantId(UUID productVariantId);

    boolean existsByProductVariantId(UUID productVariantId);
}

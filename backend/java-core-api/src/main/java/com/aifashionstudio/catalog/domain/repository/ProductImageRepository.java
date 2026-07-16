package com.aifashionstudio.catalog.domain.repository;

import com.aifashionstudio.catalog.domain.model.ProductImage;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface ProductImageRepository {

    ProductImage save(ProductImage productImage);

    Optional<ProductImage> findById(UUID id);

    List<ProductImage> findByProductIdOrderBySortOrderAsc(UUID productId);

    List<ProductImage> findByProductIdInOrderByProductIdAscThumbnailDescSortOrderAsc(List<UUID> productIds);

    Optional<ProductImage> findByProductIdAndThumbnailTrue(UUID productId);

    void deleteByProductId(UUID productId);

    void delete(ProductImage productImage);
}

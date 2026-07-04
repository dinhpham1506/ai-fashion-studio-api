package com.aifashionstudio.catalog.domain.repository;

import com.aifashionstudio.catalog.domain.model.ProductImage;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface ProductImageRepository {

    ProductImage save(ProductImage productImage);

    List<ProductImage> findByProductIdOrderBySortOrderAsc(UUID productId);

    Optional<ProductImage> findByProductIdAndThumbnailTrue(UUID productId);

    void deleteByProductId(UUID productId);
}

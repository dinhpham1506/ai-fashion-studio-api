package com.aifashionstudio.catalog.infrastructure.persistence.repository;

import com.aifashionstudio.catalog.infrastructure.persistence.entity.ProductImageJpaEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface JpaProductImageRepository extends JpaRepository<ProductImageJpaEntity, UUID> {

    List<ProductImageJpaEntity> findByProductIdOrderBySortOrderAsc(UUID productId);

    Optional<ProductImageJpaEntity> findByProductIdAndThumbnailTrue(UUID productId);

    void deleteByProductId(UUID productId);
}

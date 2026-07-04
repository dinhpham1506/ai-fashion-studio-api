package com.aifashionstudio.catalog.infrastructure.persistence.repository;

import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.infrastructure.persistence.entity.ProductVariantJpaEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface JpaProductVariantRepository extends JpaRepository<ProductVariantJpaEntity, UUID> {

    List<ProductVariantJpaEntity> findByProductId(UUID productId);

    List<ProductVariantJpaEntity> findByProductIdAndStatus(UUID productId, ProductVariantStatus status);

    Optional<ProductVariantJpaEntity> findBySku(String sku);

    boolean existsBySku(String sku);
}

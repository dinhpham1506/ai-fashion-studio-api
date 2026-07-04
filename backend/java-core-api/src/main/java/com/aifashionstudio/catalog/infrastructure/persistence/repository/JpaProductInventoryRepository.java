package com.aifashionstudio.catalog.infrastructure.persistence.repository;

import com.aifashionstudio.catalog.infrastructure.persistence.entity.ProductInventoryJpaEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;
import java.util.UUID;

public interface JpaProductInventoryRepository extends JpaRepository<ProductInventoryJpaEntity, UUID> {

    Optional<ProductInventoryJpaEntity> findByProductVariantId(UUID productVariantId);

    boolean existsByProductVariantId(UUID productVariantId);
}

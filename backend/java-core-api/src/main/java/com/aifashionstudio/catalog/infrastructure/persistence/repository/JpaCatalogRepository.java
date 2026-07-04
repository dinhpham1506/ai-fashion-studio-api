package com.aifashionstudio.catalog.infrastructure.persistence.repository;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.infrastructure.persistence.entity.CatalogJpaEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface JpaCatalogRepository extends JpaRepository<CatalogJpaEntity, UUID> {

    List<CatalogJpaEntity> findByStatus(CatalogStatus status);

    List<CatalogJpaEntity> findByNameContainingIgnoreCase(String name);

    boolean existsByNameIgnoreCase(String name);
}

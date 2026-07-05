package com.aifashionstudio.catalog.domain.repository;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface CatalogRepository {

    Catalog save(Catalog catalog);

    Optional<Catalog> findById(UUID id);
    List<Catalog> findAll();

    List<Catalog> findByStatus(CatalogStatus status);

    List<Catalog> findByNameContainingIgnoreCase(String name);

    boolean existsByNameIgnoreCase(String name);
}

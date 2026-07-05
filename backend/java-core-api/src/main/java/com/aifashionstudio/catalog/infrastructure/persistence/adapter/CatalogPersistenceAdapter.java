package com.aifashionstudio.catalog.infrastructure.persistence.adapter;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.infrastructure.persistence.mapper.CatalogPersistenceMapper;
import com.aifashionstudio.catalog.infrastructure.persistence.repository.JpaCatalogRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class CatalogPersistenceAdapter implements CatalogRepository {

    private final JpaCatalogRepository jpaCatalogRepository;
    private final CatalogPersistenceMapper mapper;

    @Override
    public Catalog save(Catalog catalog) {
        return mapper.toDomain(jpaCatalogRepository.save(mapper.toEntity(catalog)));
    }

    @Override
    public Optional<Catalog> findById(UUID id) {
        return jpaCatalogRepository.findById(id)
                .map(mapper::toDomain);
    }

    @Override
    public List<Catalog> findAll() {
        return  jpaCatalogRepository.findAll()
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public List<Catalog> findByStatus(CatalogStatus status) {
        return jpaCatalogRepository.findByStatus(status)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public List<Catalog> findByNameContainingIgnoreCase(String name) {
        return jpaCatalogRepository.findByNameContainingIgnoreCase(name)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public boolean existsByNameIgnoreCase(String name) {
        return jpaCatalogRepository.existsByNameIgnoreCase(name);
    }
}

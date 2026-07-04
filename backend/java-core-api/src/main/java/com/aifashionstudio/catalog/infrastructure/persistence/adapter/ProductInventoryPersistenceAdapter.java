package com.aifashionstudio.catalog.infrastructure.persistence.adapter;

import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.infrastructure.persistence.mapper.CatalogPersistenceMapper;
import com.aifashionstudio.catalog.infrastructure.persistence.repository.JpaProductInventoryRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class ProductInventoryPersistenceAdapter implements ProductInventoryRepository {

    private final JpaProductInventoryRepository jpaProductInventoryRepository;
    private final CatalogPersistenceMapper mapper;

    @Override
    public ProductInventory save(ProductInventory productInventory) {
        return mapper.toDomain(jpaProductInventoryRepository.save(mapper.toEntity(productInventory)));
    }

    @Override
    public Optional<ProductInventory> findByProductVariantId(UUID productVariantId) {
        return jpaProductInventoryRepository.findByProductVariantId(productVariantId)
                .map(mapper::toDomain);
    }

    @Override
    public boolean existsByProductVariantId(UUID productVariantId) {
        return jpaProductInventoryRepository.existsByProductVariantId(productVariantId);
    }
}

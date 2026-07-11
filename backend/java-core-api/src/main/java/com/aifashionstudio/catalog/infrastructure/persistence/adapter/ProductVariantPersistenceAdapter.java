package com.aifashionstudio.catalog.infrastructure.persistence.adapter;

import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.catalog.infrastructure.persistence.mapper.CatalogPersistenceMapper;
import com.aifashionstudio.catalog.infrastructure.persistence.repository.JpaProductVariantRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class ProductVariantPersistenceAdapter implements ProductVariantRepository {

    private final JpaProductVariantRepository jpaProductVariantRepository;
    private final CatalogPersistenceMapper mapper;

    @Override
    public ProductVariant save(ProductVariant productVariant) {
        return mapper.toDomain(jpaProductVariantRepository.save(mapper.toEntity(productVariant)));
    }

    @Override
    public Optional<ProductVariant> findById(UUID id) {
        return jpaProductVariantRepository.findById(id)
                .map(mapper::toDomain);
    }

    @Override
    public List<ProductVariant> findByProductId(UUID productId) {
        return jpaProductVariantRepository.findByProductId(productId)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public List<ProductVariant> findByProductIdAndStatus(UUID productId, ProductVariantStatus status) {
        return jpaProductVariantRepository.findByProductIdAndStatus(productId, status)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public Optional<ProductVariant> findBySku(String sku) {
        return jpaProductVariantRepository.findBySku(sku)
                .map(mapper::toDomain);
    }

    @Override
    public boolean existsBySku(String sku) {
        return jpaProductVariantRepository.existsBySku(sku);
    }

    @Override
    public boolean existsBySkuAndIdNot(String sku, UUID id) {
        return jpaProductVariantRepository.existsBySkuAndIdNot(sku, id);
    }
}

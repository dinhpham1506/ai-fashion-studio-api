package com.aifashionstudio.catalog.infrastructure.persistence.adapter;

import com.aifashionstudio.catalog.domain.model.ProductImage;
import com.aifashionstudio.catalog.domain.repository.ProductImageRepository;
import com.aifashionstudio.catalog.infrastructure.persistence.mapper.CatalogPersistenceMapper;
import com.aifashionstudio.catalog.infrastructure.persistence.repository.JpaProductImageRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class ProductImagePersistenceAdapter implements ProductImageRepository {

    private final JpaProductImageRepository jpaProductImageRepository;
    private final CatalogPersistenceMapper mapper;

    @Override
    public ProductImage save(ProductImage productImage) {
        return mapper.toDomain(jpaProductImageRepository.save(mapper.toEntity(productImage)));
    }

    @Override
    public List<ProductImage> findByProductIdOrderBySortOrderAsc(UUID productId) {
        return jpaProductImageRepository.findByProductIdOrderBySortOrderAsc(productId)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public Optional<ProductImage> findByProductIdAndThumbnailTrue(UUID productId) {
        return jpaProductImageRepository.findByProductIdAndThumbnailTrue(productId)
                .map(mapper::toDomain);
    }

    @Override
    @Transactional
    public void deleteByProductId(UUID productId) {
        jpaProductImageRepository.deleteByProductId(productId);
    }
}

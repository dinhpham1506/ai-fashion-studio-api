package com.aifashionstudio.catalog.domain.service.impl;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.service.CatalogDomainService;

import java.math.BigDecimal;

public class CatalogDomainServiceImpl implements CatalogDomainService {

    private final CatalogRepository catalogRepository;

    public CatalogDomainServiceImpl(CatalogRepository catalogRepository) {
        this.catalogRepository = catalogRepository;
    }

    @Override
    public void validateCatalogCanBeCreated(String name, BigDecimal basePrice) {
        if (name == null || name.isBlank()) {
            throw new IllegalArgumentException("Catalog name cannot be null or empty");
        }

        if (basePrice == null || basePrice.compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("Base price cannot be null or negative");
        }

        if (catalogRepository.existsByNameIgnoreCase(name.trim())) {
            throw new IllegalArgumentException("Catalog with the same name already exists");
        }
    }

    @Override
    public void validateCatalogCanBeActivated(Catalog catalog) {
        if (catalog == null) {
            throw new IllegalArgumentException("Catalog cannot be null");
        }

        if (catalog.getStatus() != CatalogStatus.INACTIVE) {
            throw new IllegalArgumentException("Catalog must be in INACTIVE status to be activated");
        }
    }

    @Override
    public BigDecimal calculateVariantFinalPrice(BigDecimal basePrice, ProductVariant productVariant) {
        if (basePrice == null || basePrice.compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("Base price cannot be null or negative");
        }

        if (productVariant == null) {
            throw new IllegalArgumentException("Product variant cannot be null");
        }

        if (productVariant.getPriceAdjustment() == null
                || productVariant.getPriceAdjustment().compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("Variant price adjustment cannot be null or negative");
        }

        return basePrice.add(productVariant.getPriceAdjustment());
    }
}

package com.aifashionstudio.catalog.domain.service;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.ProductVariant;

import java.math.BigDecimal;

public interface CatalogDomainService {

    void validateCatalogCanBeCreated(String name, BigDecimal basePrice);

    void validateCatalogCanBeActivated(Catalog catalog);

    BigDecimal calculateVariantFinalPrice(BigDecimal basePrice, ProductVariant productVariant);
}

package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.CreateProductVariantCommand;
import com.aifashionstudio.catalog.application.command.UpdateProductVariantCommand;
import com.aifashionstudio.catalog.application.dto.ProductInventoryResult;
import com.aifashionstudio.catalog.application.dto.ProductVariantResult;
import com.aifashionstudio.catalog.application.mapper.ProductCatalogApplicationMapper;
import com.aifashionstudio.catalog.application.service.ProductVariantApplicationService;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ConflictException;
import com.aifashionstudio.shared.exception.NotFoundException;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ProductVariantApplicationServiceImpl implements ProductVariantApplicationService {

    private final CatalogRepository catalogRepository;
    private final ProductVariantRepository productVariantRepository;
    private final ProductInventoryRepository productInventoryRepository;
    private final ProductCatalogApplicationMapper mapper;

    @Override
    @Transactional
    public ProductVariantResult createVariant(UUID productId, CreateProductVariantCommand command) {
        Catalog product = getCatalogOrThrow(productId);
        String sku = normalizeRequired(command.sku(), "SKU cannot be null or empty");
        validateVariantDetails(command.size(), command.color(), command.material(), command.priceAdjustment());

        if (productVariantRepository.existsBySku(sku)) {
            throw new ConflictException("PRODUCT_VARIANT_SKU_EXISTS", "Product variant SKU already exists: " + sku);
        }

        ProductVariant variant = new ProductVariant();
        variant.setProduct(product);
        variant.setSku(sku);
        variant.setSize(command.size().trim());
        variant.setColor(command.color().trim());
        variant.setMaterial(command.material().trim());
        variant.setPriceAdjustment(command.priceAdjustment());
        variant.setStatus(ProductVariantStatus.ACTIVE);

        ProductVariant savedVariant = productVariantRepository.save(variant);
        ProductInventory inventory = createDefaultInventory(savedVariant);

        return mapper.toVariantResult(savedVariant, mapper.toInventoryResult(inventory));
    }

    @Override
    @Transactional
    public ProductVariantResult updateVariant(UUID variantId, UpdateProductVariantCommand command) {
        ProductVariant variant = getVariantOrThrow(variantId);
        String sku = normalizeRequired(command.sku(), "SKU cannot be null or empty");
        validateVariantDetails(command.size(), command.color(), command.material(), command.priceAdjustment());

        if (productVariantRepository.existsBySkuAndIdNot(sku, variantId)) {
            throw new ConflictException("PRODUCT_VARIANT_SKU_EXISTS", "Product variant SKU already exists: " + sku);
        }

        variant.setSku(sku);
        variant.setSize(command.size().trim());
        variant.setColor(command.color().trim());
        variant.setMaterial(command.material().trim());
        variant.setPriceAdjustment(command.priceAdjustment());

        return toResult(productVariantRepository.save(variant));
    }

    @Override
    @Transactional
    public ProductVariantResult changeVariantStatus(UUID variantId, ProductVariantStatus status) {
        if (status == null) {
            throw new BusinessRuleException("INVALID_PRODUCT_VARIANT_STATUS", "Product variant status cannot be null");
        }

        ProductVariant variant = getVariantOrThrow(variantId);
        variant.setStatus(status);

        return toResult(productVariantRepository.save(variant));
    }

    @Override
    public ProductVariantResult getVariantById(UUID variantId) {
        return toResult(getVariantOrThrow(variantId));
    }

    @Override
    public ProductVariantResult getPublicVariantById(UUID variantId) {
        ProductVariant variant = getVariantOrThrow(variantId);
        assertPublicVariant(variant);
        return toResult(variant);
    }

    @Override
    public List<ProductVariantResult> getVariantsByProductId(UUID productId) {
        getCatalogOrThrow(productId);
        return productVariantRepository.findByProductId(productId)
                .stream()
                .map(this::toResult)
                .toList();
    }

    @Override
    public List<ProductVariantResult> getPublicVariantsByProductId(UUID productId) {
        Catalog product = getCatalogOrThrow(productId);
        if (product.getStatus() != CatalogStatus.ACTIVE) {
            throw productNotFound(productId);
        }

        return productVariantRepository.findByProductIdAndStatus(productId, ProductVariantStatus.ACTIVE)
                .stream()
                .map(this::toResult)
                .toList();
    }

    private ProductVariantResult toResult(ProductVariant variant) {
        ProductInventoryResult inventory = productInventoryRepository.findByProductVariantId(variant.getId())
                .map(mapper::toInventoryResult)
                .orElseGet(() -> mapper.zeroInventoryResult(variant));

        return mapper.toVariantResult(variant, inventory);
    }

    private ProductInventory createDefaultInventory(ProductVariant variant) {
        ProductInventory inventory = new ProductInventory();
        inventory.setProductVariant(variant);
        inventory.setAvailableQuantity(0);
        inventory.setReservedQuantity(0);
        inventory.setSoldQuantity(0);
        return productInventoryRepository.save(inventory);
    }

    private ProductVariant getVariantOrThrow(UUID variantId) {
        return productVariantRepository.findById(variantId)
                .orElseThrow(() -> new NotFoundException(
                        "PRODUCT_VARIANT_NOT_FOUND",
                        "Product variant not found with id: " + variantId
                ));
    }

    private Catalog getCatalogOrThrow(UUID productId) {
        return catalogRepository.findById(productId)
                .orElseThrow(() -> new NotFoundException(
                        "CATALOG_NOT_FOUND",
                        "Catalog not found with id: " + productId
                ));
    }

    private void assertPublicVariant(ProductVariant variant) {
        if (variant.getStatus() != ProductVariantStatus.ACTIVE
                || variant.getProduct().getStatus() != CatalogStatus.ACTIVE) {
            throw productVariantNotFound(variant.getId());
        }
    }

    private void validateVariantDetails(String size, String color, String material, BigDecimal priceAdjustment) {
        normalizeRequired(size, "Product variant size cannot be null or empty");
        normalizeRequired(color, "Product variant color cannot be null or empty");
        normalizeRequired(material, "Product variant material cannot be null or empty");

        if (priceAdjustment == null || priceAdjustment.compareTo(BigDecimal.ZERO) < 0) {
            throw new BusinessRuleException(
                    "INVALID_PRODUCT_VARIANT_PRICE",
                    "Product variant price adjustment cannot be null or negative"
            );
        }
    }

    private String normalizeRequired(String value, String message) {
        if (value == null || value.isBlank()) {
            throw new BusinessRuleException("INVALID_PRODUCT_VARIANT", message);
        }
        return value.trim();
    }

    private NotFoundException productNotFound(UUID productId) {
        return new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + productId);
    }

    private NotFoundException productVariantNotFound(UUID variantId) {
        return new NotFoundException("PRODUCT_VARIANT_NOT_FOUND", "Product variant not found with id: " + variantId);
    }
}

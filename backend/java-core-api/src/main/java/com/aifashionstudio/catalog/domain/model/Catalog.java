package com.aifashionstudio.catalog.domain.model;

import com.aifashionstudio.shared.domain.model.common.AuditableDomainModel;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

public class Catalog extends AuditableDomainModel {

    private String name;
    private String description;
    private BigDecimal basePrice;
    private CatalogStatus status;
    private UUID createdBy;
    private List<ProductVariant> variants = new ArrayList<>();
    private List<ProductImage> images = new ArrayList<>();

    protected Catalog() {} // để framework persistence dùng nếu cần, không public

    public static Catalog create(String name, String description, BigDecimal basePrice) {
        validateName(name);
        validateBasePrice(basePrice);

        Catalog catalog = new Catalog();
        catalog.name = name.trim();
        catalog.description = description;
        catalog.basePrice = basePrice;
        catalog.status = CatalogStatus.DRAFT;
        return catalog;
    }

    public static Catalog reconstitute(UUID id,
                                       String name,
                                       String description,
                                       BigDecimal basePrice,
                                       CatalogStatus status,
                                       UUID createdBy,
                                       OffsetDateTime createdAt,
                                       OffsetDateTime updatedAt) {
        Catalog catalog = new Catalog();
        catalog.setId(id);
        catalog.name = name;
        catalog.description = description;
        catalog.basePrice = basePrice;
        catalog.status = status;
        catalog.createdBy = createdBy;
        catalog.setCreatedAt(createdAt);
        catalog.setUpdatedAt(updatedAt);
        return catalog;
    }

    // Behavior method thay vì setter trần
    public void changeStatus(CatalogStatus newStatus) {
        if (newStatus == null) {
            throw new IllegalArgumentException("Catalog status cannot be null");
        }
        if (this.status == CatalogStatus.ARCHIVED) {
            throw new IllegalStateException("Catalog is archived and cannot change status");
        }
        if (this.status == CatalogStatus.DRAFT && newStatus == CatalogStatus.ARCHIVED) {
            throw new IllegalStateException("Catalog must be active before it can be archived");
        }
        this.status = newStatus;
    }

    public void updateDetails(String name, String description, BigDecimal basePrice) {
        if (this.status == CatalogStatus.ARCHIVED) {
            throw new IllegalStateException("Catalog is archived and cannot be updated");
        }
        validateName(name);
        validateBasePrice(basePrice);
        this.name = name.trim();
        this.description = description;
        this.basePrice = basePrice;
    }

    public void updatePrice(BigDecimal newPrice) {
        validateBasePrice(newPrice);
        this.basePrice = newPrice;
    }

    public void addVariant(ProductVariant variant) {
        // rule kiểm tra trước khi add, ví dụ check trùng SKU
        this.variants.add(variant);
    }

    // Trả về unmodifiable list, không cho sửa trực tiếp từ ngoài
    public List<ProductVariant> getVariants() {
        return Collections.unmodifiableList(variants);
    }

    public List<ProductImage> getImages() {
        return Collections.unmodifiableList(images);
    }

    public String getName() { return name; }
    public String getDescription() { return description; }
    public BigDecimal getBasePrice() { return basePrice; }
    public CatalogStatus getStatus() { return status; }
    public UUID getCreatedBy() { return createdBy; }

    private static void validateName(String name) {
        if (name == null || name.isBlank()) {
            throw new IllegalArgumentException("Catalog name cannot be null or empty");
        }
    }

    private static void validateBasePrice(BigDecimal basePrice) {
        if (basePrice == null || basePrice.compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("Base price cannot be null or negative");
        }
    }
}

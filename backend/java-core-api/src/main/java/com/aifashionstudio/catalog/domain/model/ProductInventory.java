package com.aifashionstudio.catalog.domain.model;

import com.aifashionstudio.shared.domain.model.common.BaseDomainModel;

import java.time.OffsetDateTime;
import java.util.UUID;

public class ProductInventory extends BaseDomainModel {

    private ProductVariant productVariant;
    private int availableQuantity;
    private int reservedQuantity;
    private int soldQuantity;
    private OffsetDateTime updatedAt;

    public ProductInventory() {
    }

    public static ProductInventory reconstitute(UUID id,
                                                ProductVariant productVariant,
                                                int availableQuantity,
                                                int reservedQuantity,
                                                int soldQuantity,
                                                OffsetDateTime updatedAt) {
        ProductInventory productInventory = new ProductInventory();
        productInventory.setId(id);
        productInventory.productVariant = productVariant;
        productInventory.availableQuantity = availableQuantity;
        productInventory.reservedQuantity = reservedQuantity;
        productInventory.soldQuantity = soldQuantity;
        productInventory.updatedAt = updatedAt;
        return productInventory;
    }

    public ProductVariant getProductVariant() {
        return productVariant;
    }

    public void setProductVariant(ProductVariant productVariant) {
        this.productVariant = productVariant;
    }

    public int getAvailableQuantity() {
        return availableQuantity;
    }

    public void setAvailableQuantity(int availableQuantity) {
        this.availableQuantity = availableQuantity;
    }

    public int getReservedQuantity() {
        return reservedQuantity;
    }

    public void setReservedQuantity(int reservedQuantity) {
        this.reservedQuantity = reservedQuantity;
    }

    public int getSoldQuantity() {
        return soldQuantity;
    }

    public void setSoldQuantity(int soldQuantity) {
        this.soldQuantity = soldQuantity;
    }

    public void reserve(int quantity) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }
        if (availableQuantity < quantity) {
            throw new IllegalStateException("Product is out of stock");
        }
        availableQuantity -= quantity;
        reservedQuantity += quantity;
        updatedAt = OffsetDateTime.now();
    }

    public void markSoldFromReserved(int quantity) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }
        if (reservedQuantity < quantity) {
            throw new IllegalStateException("Reserved quantity is not enough");
        }
        reservedQuantity -= quantity;
        soldQuantity += quantity;
        updatedAt = OffsetDateTime.now();
    }

    public OffsetDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(OffsetDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }
}

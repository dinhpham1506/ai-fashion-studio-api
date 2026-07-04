package com.aifashionstudio.catalog.domain.model;

import com.aifashionstudio.shared.domain.model.common.AuditableDomainModel;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public class ProductVariant extends AuditableDomainModel {

    private Catalog product;
    private String sku;
    private String size;
    private String color;
    private String material;
    private BigDecimal priceAdjustment;
    private ProductVariantStatus status;
    private ProductInventory inventory;

    public ProductVariant() {
    }

    public static ProductVariant reconstitute(UUID id,
                                              Catalog product,
                                              String sku,
                                              String size,
                                              String color,
                                              String material,
                                              BigDecimal priceAdjustment,
                                              ProductVariantStatus status,
                                              OffsetDateTime createdAt,
                                              OffsetDateTime updatedAt) {
        ProductVariant productVariant = new ProductVariant();
        productVariant.setId(id);
        productVariant.product = product;
        productVariant.sku = sku;
        productVariant.size = size;
        productVariant.color = color;
        productVariant.material = material;
        productVariant.priceAdjustment = priceAdjustment;
        productVariant.status = status;
        productVariant.setCreatedAt(createdAt);
        productVariant.setUpdatedAt(updatedAt);
        return productVariant;
    }

    public Catalog getProduct() {
        return product;
    }

    public void setProduct(Catalog product) {
        this.product = product;
    }

    public String getSku() {
        return sku;
    }

    public void setSku(String sku) {
        this.sku = sku;
    }

    public String getSize() {
        return size;
    }

    public void setSize(String size) {
        this.size = size;
    }

    public String getColor() {
        return color;
    }

    public void setColor(String color) {
        this.color = color;
    }

    public String getMaterial() {
        return material;
    }

    public void setMaterial(String material) {
        this.material = material;
    }

    public BigDecimal getPriceAdjustment() {
        return priceAdjustment;
    }

    public void setPriceAdjustment(BigDecimal priceAdjustment) {
        this.priceAdjustment = priceAdjustment;
    }

    public ProductVariantStatus getStatus() {
        return status;
    }

    public void setStatus(ProductVariantStatus status) {
        this.status = status;
    }

    public ProductInventory getInventory() {
        return inventory;
    }

    public void setInventory(ProductInventory inventory) {
        this.inventory = inventory;
    }
}

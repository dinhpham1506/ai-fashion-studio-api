package com.aifashionstudio.catalog.domain.model;

import com.aifashionstudio.shared.domain.model.common.BaseDomainModel;

import java.time.OffsetDateTime;
import java.util.UUID;

public class ProductImage extends BaseDomainModel {

    private Catalog product;
    private String imageUrl;
    private boolean thumbnail;
    private int sortOrder;
    private OffsetDateTime createdAt;

    public ProductImage() {
    }

    public static ProductImage reconstitute(UUID id,
                                            Catalog product,
                                            String imageUrl,
                                            boolean thumbnail,
                                            int sortOrder,
                                            OffsetDateTime createdAt) {
        ProductImage productImage = new ProductImage();
        productImage.setId(id);
        productImage.product = product;
        productImage.imageUrl = imageUrl;
        productImage.thumbnail = thumbnail;
        productImage.sortOrder = sortOrder;
        productImage.createdAt = createdAt;
        return productImage;
    }

    public Catalog getProduct() {
        return product;
    }

    public void setProduct(Catalog product) {
        this.product = product;
    }

    public String getImageUrl() {
        return imageUrl;
    }

    public void setImageUrl(String imageUrl) {
        this.imageUrl = imageUrl;
    }

    public boolean isThumbnail() {
        return thumbnail;
    }

    public void setThumbnail(boolean thumbnail) {
        this.thumbnail = thumbnail;
    }

    public int getSortOrder() {
        return sortOrder;
    }

    public void setSortOrder(int sortOrder) {
        this.sortOrder = sortOrder;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }
}

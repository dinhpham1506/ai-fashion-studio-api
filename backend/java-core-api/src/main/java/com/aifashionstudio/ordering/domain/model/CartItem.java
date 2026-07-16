package com.aifashionstudio.ordering.domain.model;

import java.time.OffsetDateTime;
import java.util.UUID;

public class CartItem {

    private UUID id;
    private UUID cartId;
    private UUID productId;
    private UUID productVariantId;
    private UUID designId;
    private int quantity;
    private OffsetDateTime createdAt;
    private OffsetDateTime updatedAt;

    public static CartItem create(UUID productId, UUID productVariantId, UUID designId, int quantity) {
        validateRequired(productId, "Product id cannot be null");
        validateRequired(productVariantId, "Product variant id cannot be null");
        validateRequired(designId, "Design id cannot be null");
        validateQuantity(quantity);

        CartItem item = new CartItem();
        item.productId = productId;
        item.productVariantId = productVariantId;
        item.designId = designId;
        item.quantity = quantity;
        return item;
    }

    public static CartItem reconstitute(UUID id,
                                        UUID cartId,
                                        UUID productId,
                                        UUID productVariantId,
                                        UUID designId,
                                        int quantity,
                                        OffsetDateTime createdAt,
                                        OffsetDateTime updatedAt) {
        CartItem item = new CartItem();
        item.id = id;
        item.cartId = cartId;
        item.productId = productId;
        item.productVariantId = productVariantId;
        item.designId = designId;
        item.quantity = quantity;
        item.createdAt = createdAt;
        item.updatedAt = updatedAt;
        return item;
    }

    public void updateQuantity(int quantity) {
        validateQuantity(quantity);
        this.quantity = quantity;
    }

    public boolean matches(UUID productId, UUID productVariantId, UUID designId) {
        return this.productId.equals(productId)
                && this.productVariantId.equals(productVariantId)
                && this.designId.equals(designId);
    }

    public UUID getId() {
        return id;
    }

    public UUID getCartId() {
        return cartId;
    }

    public void setCartId(UUID cartId) {
        this.cartId = cartId;
    }

    public UUID getProductId() {
        return productId;
    }

    public UUID getProductVariantId() {
        return productVariantId;
    }

    public UUID getDesignId() {
        return designId;
    }

    public int getQuantity() {
        return quantity;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public OffsetDateTime getUpdatedAt() {
        return updatedAt;
    }

    private static void validateRequired(UUID value, String message) {
        if (value == null) {
            throw new IllegalArgumentException(message);
        }
    }

    private static void validateQuantity(int quantity) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }
    }
}

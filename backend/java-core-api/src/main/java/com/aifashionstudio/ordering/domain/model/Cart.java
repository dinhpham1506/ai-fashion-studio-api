package com.aifashionstudio.ordering.domain.model;

import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

public class Cart {

    private UUID id;
    private UUID customerId;
    private OffsetDateTime createdAt;
    private OffsetDateTime updatedAt;
    private List<CartItem> items = new ArrayList<>();

    public static Cart create(UUID customerId) {
        if (customerId == null) {
            throw new IllegalArgumentException("Customer id cannot be null");
        }

        Cart cart = new Cart();
        cart.customerId = customerId;
        return cart;
    }

    public static Cart reconstitute(UUID id,
                                    UUID customerId,
                                    OffsetDateTime createdAt,
                                    OffsetDateTime updatedAt,
                                    List<CartItem> items) {
        Cart cart = new Cart();
        cart.id = id;
        cart.customerId = customerId;
        cart.createdAt = createdAt;
        cart.updatedAt = updatedAt;
        cart.items = new ArrayList<>(items == null ? List.of() : items);
        cart.items.forEach(item -> item.setCartId(id));
        return cart;
    }

    public void addOrIncreaseItem(CartItem newItem) {
        CartItem existing = items.stream()
                .filter(item -> item.matches(newItem.getProductId(), newItem.getProductVariantId(), newItem.getDesignId()))
                .findFirst()
                .orElse(null);

        if (existing == null) {
            newItem.setCartId(id);
            items.add(newItem);
            return;
        }

        existing.updateQuantity(existing.getQuantity() + newItem.getQuantity());
    }

    public void updateItemQuantity(UUID itemId, int quantity) {
        findItem(itemId).updateQuantity(quantity);
    }

    public void removeItem(UUID itemId) {
        CartItem item = findItem(itemId);
        items.remove(item);
    }

    public void clear() {
        items.clear();
    }

    public CartItem findItem(UUID itemId) {
        return items.stream()
                .filter(item -> itemId.equals(item.getId()))
                .findFirst()
                .orElseThrow(() -> new IllegalArgumentException("Cart item not found"));
    }

    public UUID getId() {
        return id;
    }

    public UUID getCustomerId() {
        return customerId;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public OffsetDateTime getUpdatedAt() {
        return updatedAt;
    }

    public List<CartItem> getItems() {
        return Collections.unmodifiableList(items);
    }
}

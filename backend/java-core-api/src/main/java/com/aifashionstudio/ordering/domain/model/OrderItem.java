package com.aifashionstudio.ordering.domain.model;

import com.aifashionstudio.shared.domain.model.common.BaseDomainModel;

import java.math.BigDecimal;
import java.util.Map;
import java.util.UUID;

public class OrderItem extends BaseDomainModel {

    private UUID orderId;
    private UUID productId;
    private UUID productVariantId;
    private UUID designId;
    private String productNameSnapshot;
    private Map<String, Object> variantSnapshot;
    private int quantity;
    private BigDecimal unitPrice;
    private BigDecimal totalPrice;

    protected OrderItem() {
    }

    public static OrderItem create(UUID productId,
                                   UUID productVariantId,
                                   UUID designId,
                                   String productNameSnapshot,
                                   Map<String, Object> variantSnapshot,
                                   int quantity,
                                   BigDecimal unitPrice) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }
        if (unitPrice == null || unitPrice.compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("Unit price cannot be negative");
        }

        OrderItem item = new OrderItem();
        item.productId = productId;
        item.productVariantId = productVariantId;
        item.designId = designId;
        item.productNameSnapshot = productNameSnapshot;
        item.variantSnapshot = Map.copyOf(variantSnapshot);
        item.quantity = quantity;
        item.unitPrice = unitPrice;
        item.totalPrice = unitPrice.multiply(BigDecimal.valueOf(quantity));
        return item;
    }

    public static OrderItem reconstitute(UUID id,
                                         UUID orderId,
                                         UUID productId,
                                         UUID productVariantId,
                                         UUID designId,
                                         String productNameSnapshot,
                                         Map<String, Object> variantSnapshot,
                                         int quantity,
                                         BigDecimal unitPrice,
                                         BigDecimal totalPrice) {
        OrderItem item = new OrderItem();
        item.setId(id);
        item.orderId = orderId;
        item.productId = productId;
        item.productVariantId = productVariantId;
        item.designId = designId;
        item.productNameSnapshot = productNameSnapshot;
        item.variantSnapshot = variantSnapshot == null ? Map.of() : Map.copyOf(variantSnapshot);
        item.quantity = quantity;
        item.unitPrice = unitPrice;
        item.totalPrice = totalPrice;
        return item;
    }

    public void assignOrderId(UUID orderId) {
        this.orderId = orderId;
    }

    public UUID getOrderId() {
        return orderId;
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

    public String getProductNameSnapshot() {
        return productNameSnapshot;
    }

    public Map<String, Object> getVariantSnapshot() {
        return variantSnapshot;
    }

    public int getQuantity() {
        return quantity;
    }

    public BigDecimal getUnitPrice() {
        return unitPrice;
    }

    public BigDecimal getTotalPrice() {
        return totalPrice;
    }
}

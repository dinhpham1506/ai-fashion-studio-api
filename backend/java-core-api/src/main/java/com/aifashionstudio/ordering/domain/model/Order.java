package com.aifashionstudio.ordering.domain.model;

import com.aifashionstudio.shared.domain.model.common.AuditableDomainModel;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

public class Order extends AuditableDomainModel {

    private String orderCode;
    private UUID customerId;
    private BigDecimal totalAmount;
    private PaymentStatus paymentStatus;
    private OrderStatus orderStatus;
    private String receiverName;
    private String receiverPhone;
    private String shippingAddress;
    private List<OrderItem> items = new ArrayList<>();

    protected Order() {
    }

    public static Order create(UUID customerId,
                               String orderCode,
                               String receiverName,
                               String receiverPhone,
                               String shippingAddress,
                               List<OrderItem> items) {
        if (customerId == null) {
            throw new IllegalArgumentException("Customer id cannot be null");
        }
        validateText(orderCode, "Order code cannot be empty");
        validateText(receiverName, "Receiver name cannot be empty");
        validateText(receiverPhone, "Receiver phone cannot be empty");
        validateText(shippingAddress, "Shipping address cannot be empty");
        if (items == null || items.isEmpty()) {
            throw new IllegalArgumentException("Order items are required");
        }

        Order order = new Order();
        order.customerId = customerId;
        order.orderCode = orderCode;
        order.receiverName = receiverName.trim();
        order.receiverPhone = receiverPhone.trim();
        order.shippingAddress = shippingAddress.trim();
        order.items = new ArrayList<>(items);
        order.totalAmount = items.stream()
                .map(OrderItem::getTotalPrice)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
        order.paymentStatus = PaymentStatus.PENDING;
        order.orderStatus = OrderStatus.PENDING_PAYMENT;
        return order;
    }

    public static Order reconstitute(UUID id,
                                     String orderCode,
                                     UUID customerId,
                                     BigDecimal totalAmount,
                                     PaymentStatus paymentStatus,
                                     OrderStatus orderStatus,
                                     String receiverName,
                                     String receiverPhone,
                                     String shippingAddress,
                                     OffsetDateTime createdAt,
                                     OffsetDateTime updatedAt,
                                     List<OrderItem> items) {
        Order order = new Order();
        order.setId(id);
        order.orderCode = orderCode;
        order.customerId = customerId;
        order.totalAmount = totalAmount;
        order.paymentStatus = paymentStatus;
        order.orderStatus = orderStatus;
        order.receiverName = receiverName;
        order.receiverPhone = receiverPhone;
        order.shippingAddress = shippingAddress;
        order.setCreatedAt(createdAt);
        order.setUpdatedAt(updatedAt);
        order.items = new ArrayList<>(items == null ? List.of() : items);
        return order;
    }

    public String getOrderCode() {
        return orderCode;
    }

    public UUID getCustomerId() {
        return customerId;
    }

    public BigDecimal getTotalAmount() {
        return totalAmount;
    }

    public PaymentStatus getPaymentStatus() {
        return paymentStatus;
    }

    public OrderStatus getOrderStatus() {
        return orderStatus;
    }

    public String getReceiverName() {
        return receiverName;
    }

    public String getReceiverPhone() {
        return receiverPhone;
    }

    public String getShippingAddress() {
        return shippingAddress;
    }

    public List<OrderItem> getItems() {
        return Collections.unmodifiableList(items);
    }

    public void markPaid() {
        if (paymentStatus == PaymentStatus.PAID) {
            return;
        }
        if (orderStatus != OrderStatus.PENDING_PAYMENT) {
            throw new IllegalStateException("Only pending payment orders can be marked paid");
        }
        paymentStatus = PaymentStatus.PAID;
        orderStatus = OrderStatus.PAID;
    }

    public OrderStatus updateStatus(OrderStatus toStatus) {
        if (paymentStatus != PaymentStatus.PAID) {
            throw new IllegalStateException("Order payment is not paid");
        }
        if (!isAllowedTransition(orderStatus, toStatus)) {
            throw new IllegalArgumentException("Invalid order status transition");
        }
        OrderStatus fromStatus = orderStatus;
        orderStatus = toStatus;
        return fromStatus;
    }

    private static boolean isAllowedTransition(OrderStatus fromStatus, OrderStatus toStatus) {
        return (fromStatus == OrderStatus.PAID && toStatus == OrderStatus.IN_PRODUCTION)
                || (fromStatus == OrderStatus.IN_PRODUCTION && toStatus == OrderStatus.SHIPPING)
                || (fromStatus == OrderStatus.SHIPPING && toStatus == OrderStatus.COMPLETED)
                || (fromStatus == OrderStatus.PAID && toStatus == OrderStatus.CANCELLED);
    }

    private static void validateText(String value, String message) {
        if (value == null || value.isBlank()) {
            throw new IllegalArgumentException(message);
        }
    }
}

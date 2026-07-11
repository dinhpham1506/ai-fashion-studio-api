package com.aifashionstudio.ordering.domain.model;

import com.aifashionstudio.shared.domain.model.common.BaseDomainModel;

import java.time.OffsetDateTime;
import java.util.UUID;

public class OrderStatusHistory extends BaseDomainModel {

    private UUID orderId;
    private OrderStatus fromStatus;
    private OrderStatus toStatus;
    private UUID changedBy;
    private String note;
    private OffsetDateTime createdAt;

    protected OrderStatusHistory() {
    }

    public static OrderStatusHistory create(UUID orderId, OrderStatus fromStatus, OrderStatus toStatus, UUID changedBy, String note) {
        OrderStatusHistory history = new OrderStatusHistory();
        history.orderId = orderId;
        history.fromStatus = fromStatus;
        history.toStatus = toStatus;
        history.changedBy = changedBy;
        history.note = note;
        history.createdAt = OffsetDateTime.now();
        return history;
    }

    public static OrderStatusHistory reconstitute(UUID id,
                                                  UUID orderId,
                                                  OrderStatus fromStatus,
                                                  OrderStatus toStatus,
                                                  UUID changedBy,
                                                  String note,
                                                  OffsetDateTime createdAt) {
        OrderStatusHistory history = create(orderId, fromStatus, toStatus, changedBy, note);
        history.setId(id);
        history.createdAt = createdAt;
        return history;
    }

    public UUID getOrderId() {
        return orderId;
    }

    public OrderStatus getFromStatus() {
        return fromStatus;
    }

    public OrderStatus getToStatus() {
        return toStatus;
    }

    public UUID getChangedBy() {
        return changedBy;
    }

    public String getNote() {
        return note;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }
}

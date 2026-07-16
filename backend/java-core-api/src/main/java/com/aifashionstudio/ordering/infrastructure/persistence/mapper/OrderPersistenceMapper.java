package com.aifashionstudio.ordering.infrastructure.persistence.mapper;

import com.aifashionstudio.ordering.domain.model.Order;
import com.aifashionstudio.ordering.domain.model.OrderItem;
import com.aifashionstudio.ordering.domain.model.OrderStatusHistory;
import com.aifashionstudio.ordering.infrastructure.persistence.entity.OrderItemJpaEntity;
import com.aifashionstudio.ordering.infrastructure.persistence.entity.OrderJpaEntity;
import com.aifashionstudio.ordering.infrastructure.persistence.entity.OrderStatusHistoryJpaEntity;
import org.springframework.stereotype.Component;

@Component
public class OrderPersistenceMapper {

    public Order toDomain(OrderJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return Order.reconstitute(
                entity.getId(),
                entity.getOrderCode(),
                entity.getCustomerId(),
                entity.getTotalAmount(),
                entity.getPaymentStatus(),
                entity.getOrderStatus(),
                entity.getReceiverName(),
                entity.getReceiverPhone(),
                entity.getShippingAddress(),
                entity.getCreatedAt(),
                entity.getUpdatedAt(),
                entity.getItems().stream()
                        .map(this::toDomain)
                        .toList()
        );
    }

    public OrderJpaEntity toEntity(Order domain) {
        if (domain == null) {
            return null;
        }

        OrderJpaEntity entity = OrderJpaEntity.builder()
                .id(domain.getId())
                .orderCode(domain.getOrderCode())
                .customerId(domain.getCustomerId())
                .totalAmount(domain.getTotalAmount())
                .paymentStatus(domain.getPaymentStatus())
                .orderStatus(domain.getOrderStatus())
                .receiverName(domain.getReceiverName())
                .receiverPhone(domain.getReceiverPhone())
                .shippingAddress(domain.getShippingAddress())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .build();
        entity.replaceItems(domain.getItems().stream()
                .map(this::toEntity)
                .toList());
        return entity;
    }

    public OrderItem toDomain(OrderItemJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return OrderItem.reconstitute(
                entity.getId(),
                entity.getOrder() == null ? null : entity.getOrder().getId(),
                entity.getProductId(),
                entity.getProductVariantId(),
                entity.getDesignId(),
                entity.getProductNameSnapshot(),
                entity.getVariantSnapshot(),
                entity.getQuantity(),
                entity.getUnitPrice(),
                entity.getTotalPrice()
        );
    }

    public OrderItemJpaEntity toEntity(OrderItem domain) {
        if (domain == null) {
            return null;
        }

        return OrderItemJpaEntity.builder()
                .id(domain.getId())
                .productId(domain.getProductId())
                .productVariantId(domain.getProductVariantId())
                .designId(domain.getDesignId())
                .productNameSnapshot(domain.getProductNameSnapshot())
                .variantSnapshot(domain.getVariantSnapshot())
                .quantity(domain.getQuantity())
                .unitPrice(domain.getUnitPrice())
                .totalPrice(domain.getTotalPrice())
                .build();
    }

    public OrderStatusHistoryJpaEntity toEntity(OrderStatusHistory domain) {
        if (domain == null) {
            return null;
        }

        return OrderStatusHistoryJpaEntity.builder()
                .id(domain.getId())
                .orderId(domain.getOrderId())
                .fromStatus(domain.getFromStatus())
                .toStatus(domain.getToStatus())
                .changedBy(domain.getChangedBy())
                .note(domain.getNote())
                .createdAt(domain.getCreatedAt())
                .build();
    }

    public OrderStatusHistory toDomain(OrderStatusHistoryJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return OrderStatusHistory.reconstitute(
                entity.getId(),
                entity.getOrderId(),
                entity.getFromStatus(),
                entity.getToStatus(),
                entity.getChangedBy(),
                entity.getNote(),
                entity.getCreatedAt()
        );
    }
}

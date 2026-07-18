package com.aifashionstudio.ordering.infrastructure.persistence.mapper;

import com.aifashionstudio.ordering.domain.model.Cart;
import com.aifashionstudio.ordering.domain.model.CartItem;
import com.aifashionstudio.ordering.infrastructure.persistence.entity.CartItemJpaEntity;
import com.aifashionstudio.ordering.infrastructure.persistence.entity.CartJpaEntity;
import org.springframework.stereotype.Component;

@Component
public class CartPersistenceMapper {

    public Cart toDomain(CartJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return Cart.reconstitute(
                entity.getId(),
                entity.getCustomerId(),
                entity.getCreatedAt(),
                entity.getUpdatedAt(),
                entity.getItems().stream()
                        .map(this::toDomain)
                        .toList()
        );
    }

    public CartJpaEntity toEntity(Cart domain) {
        if (domain == null) {
            return null;
        }

        CartJpaEntity entity = CartJpaEntity.builder()
                .id(domain.getId())
                .customerId(domain.getCustomerId())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .build();
        entity.replaceItems(domain.getItems().stream()
                .map(this::toEntity)
                .toList());
        return entity;
    }

    public CartItem toDomain(CartItemJpaEntity entity) {
        if (entity == null) {
            return null;
        }

        return CartItem.reconstitute(
                entity.getId(),
                entity.getCart() == null ? null : entity.getCart().getId(),
                entity.getProductId(),
                entity.getProductVariantId(),
                entity.getDesignId(),
                entity.getQuantity(),
                entity.getCreatedAt(),
                entity.getUpdatedAt()
        );
    }

    public CartItemJpaEntity toEntity(CartItem domain) {
        if (domain == null) {
            return null;
        }

        return CartItemJpaEntity.builder()
                .id(domain.getId())
                .productId(domain.getProductId())
                .productVariantId(domain.getProductVariantId())
                .designId(domain.getDesignId())
                .quantity(domain.getQuantity())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .build();
    }
}

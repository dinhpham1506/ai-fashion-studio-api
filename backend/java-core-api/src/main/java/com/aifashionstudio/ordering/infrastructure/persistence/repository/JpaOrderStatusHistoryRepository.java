package com.aifashionstudio.ordering.infrastructure.persistence.repository;

import com.aifashionstudio.ordering.infrastructure.persistence.entity.OrderStatusHistoryJpaEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface JpaOrderStatusHistoryRepository extends JpaRepository<OrderStatusHistoryJpaEntity, UUID> {

    List<OrderStatusHistoryJpaEntity> findByOrderIdOrderByCreatedAtAsc(UUID orderId);
}

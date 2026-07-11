package com.aifashionstudio.ordering.infrastructure.persistence.repository;

import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.infrastructure.persistence.entity.OrderJpaEntity;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.EntityGraph;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.UUID;

public interface JpaOrderRepository extends JpaRepository<OrderJpaEntity, UUID> {

    @Override
    @EntityGraph(attributePaths = "items")
    java.util.Optional<OrderJpaEntity> findById(UUID id);

    @EntityGraph(attributePaths = "items")
    Page<OrderJpaEntity> findByCustomerIdOrderByCreatedAtDesc(UUID customerId, Pageable pageable);

    long countByCustomerId(UUID customerId);

    @EntityGraph(attributePaths = "items")
    Page<OrderJpaEntity> findByOrderStatusOrderByCreatedAtAsc(OrderStatus orderStatus, Pageable pageable);

    long countByOrderStatus(OrderStatus orderStatus);
}

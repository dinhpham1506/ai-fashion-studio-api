package com.aifashionstudio.ordering.infrastructure.persistence.repository;

import com.aifashionstudio.ordering.infrastructure.persistence.entity.CartJpaEntity;
import org.springframework.data.jpa.repository.EntityGraph;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;
import java.util.UUID;

public interface JpaCartRepository extends JpaRepository<CartJpaEntity, UUID> {

    @EntityGraph(attributePaths = "items")
    Optional<CartJpaEntity> findByCustomerId(UUID customerId);

    void deleteByCustomerId(UUID customerId);
}

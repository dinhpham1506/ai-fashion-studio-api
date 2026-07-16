package com.aifashionstudio.design.infrastructure.persistence.repository;

import com.aifashionstudio.design.infrastructure.persistence.entity.DesignJpaEntity;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.UUID;

public interface JpaDesignRepository extends JpaRepository<DesignJpaEntity, UUID> {

    Page<DesignJpaEntity> findByCustomerIdOrderByCreatedAtDesc(UUID customerId, Pageable pageable);

    long countByCustomerId(UUID customerId);
}

package com.aifashionstudio.design.infrastructure.persistence.repository;

import com.aifashionstudio.design.infrastructure.persistence.entity.DesignLayerJpaEntity;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.UUID;

public interface JpaDesignLayerRepository extends JpaRepository<DesignLayerJpaEntity, UUID> {

    void deleteByDesignId(UUID designId);

    @Query("select layer from DesignLayerJpaEntity layer where layer.designId = :designId order by layer.zIndex asc")
    List<DesignLayerJpaEntity> findByDesignIdOrderByZIndexAsc(@Param("designId") UUID designId);
}

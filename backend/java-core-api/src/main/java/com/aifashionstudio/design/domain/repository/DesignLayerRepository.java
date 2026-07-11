package com.aifashionstudio.design.domain.repository;

import com.aifashionstudio.design.domain.model.DesignLayer;

import java.util.List;
import java.util.UUID;

public interface DesignLayerRepository {

    List<DesignLayer> saveAll(List<DesignLayer> layers);

    void deleteByDesignId(UUID designId);

    List<DesignLayer> findByDesignIdOrderByZIndexAsc(UUID designId);
}

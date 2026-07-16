package com.aifashionstudio.design.infrastructure.persistence.adapter;

import com.aifashionstudio.design.domain.model.DesignLayer;
import com.aifashionstudio.design.domain.repository.DesignLayerRepository;
import com.aifashionstudio.design.infrastructure.persistence.mapper.DesignPersistenceMapper;
import com.aifashionstudio.design.infrastructure.persistence.repository.JpaDesignLayerRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class DesignLayerPersistenceAdapter implements DesignLayerRepository {

    private final JpaDesignLayerRepository jpaDesignLayerRepository;
    private final DesignPersistenceMapper mapper;

    @Override
    public List<DesignLayer> saveAll(List<DesignLayer> layers) {
        return jpaDesignLayerRepository.saveAll(layers.stream()
                        .map(mapper::toEntity)
                        .toList())
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public void deleteByDesignId(UUID designId) {
        jpaDesignLayerRepository.deleteByDesignId(designId);
    }

    @Override
    public List<DesignLayer> findByDesignIdOrderByZIndexAsc(UUID designId) {
        return jpaDesignLayerRepository.findByDesignIdOrderByZIndexAsc(designId)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }
}

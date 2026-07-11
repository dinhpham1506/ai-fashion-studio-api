package com.aifashionstudio.design.infrastructure.persistence.adapter;

import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.repository.DesignRepository;
import com.aifashionstudio.design.infrastructure.persistence.mapper.DesignPersistenceMapper;
import com.aifashionstudio.design.infrastructure.persistence.repository.JpaDesignRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class DesignPersistenceAdapter implements DesignRepository {

    private final JpaDesignRepository jpaDesignRepository;
    private final DesignPersistenceMapper mapper;

    @Override
    public Design save(Design design) {
        return mapper.toDomain(jpaDesignRepository.save(mapper.toEntity(design)));
    }

    @Override
    public Optional<Design> findById(UUID id) {
        return jpaDesignRepository.findById(id)
                .map(mapper::toDomain);
    }

    @Override
    public List<Design> findByCustomerId(UUID customerId, int page, int pageSize) {
        return jpaDesignRepository.findByCustomerIdOrderByCreatedAtDesc(customerId, PageRequest.of(page - 1, pageSize))
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public long countByCustomerId(UUID customerId) {
        return jpaDesignRepository.countByCustomerId(customerId);
    }
}

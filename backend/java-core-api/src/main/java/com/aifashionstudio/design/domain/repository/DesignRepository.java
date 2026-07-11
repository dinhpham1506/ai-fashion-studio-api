package com.aifashionstudio.design.domain.repository;

import com.aifashionstudio.design.domain.model.Design;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface DesignRepository {

    Design save(Design design);

    Optional<Design> findById(UUID id);

    List<Design> findByCustomerId(UUID customerId, int page, int pageSize);

    long countByCustomerId(UUID customerId);
}

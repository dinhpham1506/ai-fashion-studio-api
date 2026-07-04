package com.aifashionstudio.catalog.application.mapper;

import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.domain.model.Catalog;
import org.springframework.stereotype.Component;

@Component
public class CatalogApplicationMapper {

    public CatalogResult toResult(Catalog catalog) {
        return new CatalogResult(
                catalog.getId(),
                catalog.getName(),
                catalog.getDescription(),
                catalog.getBasePrice(),
                catalog.getStatus(),
                catalog.getCreatedAt(),
                catalog.getUpdatedAt()
        );
    }
}

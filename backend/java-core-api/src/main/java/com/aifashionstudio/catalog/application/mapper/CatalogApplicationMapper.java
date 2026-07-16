package com.aifashionstudio.catalog.application.mapper;

import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.domain.model.Catalog;
import org.springframework.stereotype.Component;

@Component
public class CatalogApplicationMapper {

    public CatalogResult toResult(Catalog catalog) {
        return toResult(catalog, null);
    }

    public CatalogResult toResult(Catalog catalog, String thumbnailUrl) {
        return new CatalogResult(
                catalog.getId(),
                catalog.getName(),
                catalog.getDescription(),
                catalog.getBasePrice(),
                catalog.getStatus(),
                thumbnailUrl,
                catalog.getCreatedAt(),
                catalog.getUpdatedAt()
        );
    }
}

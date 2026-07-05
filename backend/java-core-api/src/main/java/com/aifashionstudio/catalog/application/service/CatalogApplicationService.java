package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.command.CreateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;

import java.util.List;
import java.util.UUID;

public interface CatalogApplicationService {

    CatalogResult createCatalog(CreateCatalogCommand command);
    CatalogResult getCatalogById(UUID id);

    List<CatalogResult> getCatalogs();


}

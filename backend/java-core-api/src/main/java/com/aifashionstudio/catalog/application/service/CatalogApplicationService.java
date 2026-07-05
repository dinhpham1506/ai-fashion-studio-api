package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.command.CreateCatalogCommand;
import com.aifashionstudio.catalog.application.command.ChangeCatalogStatusCommand;
import com.aifashionstudio.catalog.application.command.UpdateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.util.List;
import java.util.UUID;

public interface CatalogApplicationService {

    CatalogResult createCatalog(CreateCatalogCommand command);

    CatalogResult updateCatalog(UpdateCatalogCommand command);

    CatalogResult changeCatalogStatus(ChangeCatalogStatusCommand command);

    CatalogResult getCatalogById(UUID id);

    CatalogResult getPublicProductById(UUID id);

    List<CatalogResult> getCatalogs();

    List<CatalogResult> getCatalogs(CatalogStatus status, String name);

    List<CatalogResult> getPublicProducts(String name);

}

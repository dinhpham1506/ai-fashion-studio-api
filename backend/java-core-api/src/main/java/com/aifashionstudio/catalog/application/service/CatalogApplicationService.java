package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.command.CreateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;

public interface CatalogApplicationService {

    CatalogResult createCatalog(CreateCatalogCommand command);
}

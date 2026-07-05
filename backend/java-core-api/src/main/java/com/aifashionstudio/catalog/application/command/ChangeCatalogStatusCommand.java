package com.aifashionstudio.catalog.application.command;

import com.aifashionstudio.catalog.domain.model.CatalogStatus;

import java.util.UUID;

public record ChangeCatalogStatusCommand(
        UUID id,
        CatalogStatus status
) {
}

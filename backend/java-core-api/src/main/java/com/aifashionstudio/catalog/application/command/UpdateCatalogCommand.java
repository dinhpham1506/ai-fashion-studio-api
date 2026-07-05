package com.aifashionstudio.catalog.application.command;

import java.math.BigDecimal;
import java.util.UUID;

public record UpdateCatalogCommand(
        UUID id,
        String name,
        String description,
        BigDecimal basePrice
) {
}

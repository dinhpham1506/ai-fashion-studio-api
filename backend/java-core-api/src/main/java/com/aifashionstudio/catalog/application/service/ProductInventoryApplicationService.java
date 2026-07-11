package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.command.UpdateInventoryCommand;
import com.aifashionstudio.catalog.application.dto.ProductInventoryResult;

import java.util.UUID;

public interface ProductInventoryApplicationService {

    ProductInventoryResult getInventoryByVariantId(UUID variantId);

    ProductInventoryResult getPublicInventoryByVariantId(UUID variantId);

    ProductInventoryResult updateInventory(UUID variantId, UpdateInventoryCommand command);
}

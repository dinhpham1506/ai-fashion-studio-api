package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.command.CreateProductVariantCommand;
import com.aifashionstudio.catalog.application.command.UpdateProductVariantCommand;
import com.aifashionstudio.catalog.application.dto.ProductVariantResult;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;

import java.util.List;
import java.util.UUID;

public interface ProductVariantApplicationService {

    ProductVariantResult createVariant(UUID productId, CreateProductVariantCommand command);

    ProductVariantResult updateVariant(UUID variantId, UpdateProductVariantCommand command);

    ProductVariantResult changeVariantStatus(UUID variantId, ProductVariantStatus status);

    ProductVariantResult getVariantById(UUID variantId);

    ProductVariantResult getPublicVariantById(UUID variantId);

    List<ProductVariantResult> getVariantsByProductId(UUID productId);

    List<ProductVariantResult> getPublicVariantsByProductId(UUID productId);
}

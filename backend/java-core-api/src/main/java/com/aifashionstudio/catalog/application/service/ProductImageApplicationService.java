package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.command.CreateProductImageCommand;
import com.aifashionstudio.catalog.application.command.UpdateProductImageCommand;
import com.aifashionstudio.catalog.application.dto.ProductImageResult;

import java.util.List;
import java.util.UUID;

public interface ProductImageApplicationService {

    ProductImageResult addImage(UUID productId, CreateProductImageCommand command);

    ProductImageResult updateImage(UUID productId, UUID imageId, UpdateProductImageCommand command);

    List<ProductImageResult> getImagesByProductId(UUID productId);

    List<ProductImageResult> getPublicImagesByProductId(UUID productId);

    void deleteImage(UUID productId, UUID imageId);
}

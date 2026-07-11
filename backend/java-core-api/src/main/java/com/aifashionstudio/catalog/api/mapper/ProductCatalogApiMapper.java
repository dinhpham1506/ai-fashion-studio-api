package com.aifashionstudio.catalog.api.mapper;

import com.aifashionstudio.catalog.api.dto.CreateProductImageRequest;
import com.aifashionstudio.catalog.api.dto.CreateProductVariantRequest;
import com.aifashionstudio.catalog.api.dto.InventorySummaryResponse;
import com.aifashionstudio.catalog.api.dto.ProductDetailResponse;
import com.aifashionstudio.catalog.api.dto.ProductImageResponse;
import com.aifashionstudio.catalog.api.dto.ProductInventoryResponse;
import com.aifashionstudio.catalog.api.dto.ProductVariantResponse;
import com.aifashionstudio.catalog.api.dto.UpdateInventoryRequest;
import com.aifashionstudio.catalog.api.dto.UpdateProductImageRequest;
import com.aifashionstudio.catalog.api.dto.UpdateProductVariantRequest;
import com.aifashionstudio.catalog.application.command.CreateProductImageCommand;
import com.aifashionstudio.catalog.application.command.CreateProductVariantCommand;
import com.aifashionstudio.catalog.application.command.UpdateInventoryCommand;
import com.aifashionstudio.catalog.application.command.UpdateProductImageCommand;
import com.aifashionstudio.catalog.application.command.UpdateProductVariantCommand;
import com.aifashionstudio.catalog.application.dto.ProductDetailResult;
import com.aifashionstudio.catalog.application.dto.ProductImageResult;
import com.aifashionstudio.catalog.application.dto.ProductInventoryResult;
import com.aifashionstudio.catalog.application.dto.ProductVariantResult;
import org.springframework.stereotype.Component;

@Component
public class ProductCatalogApiMapper {

    public CreateProductVariantCommand toCommand(CreateProductVariantRequest request) {
        return new CreateProductVariantCommand(
                request.sku(),
                request.size(),
                request.color(),
                request.material(),
                request.priceAdjustment()
        );
    }

    public UpdateProductVariantCommand toCommand(UpdateProductVariantRequest request) {
        return new UpdateProductVariantCommand(
                request.sku(),
                request.size(),
                request.color(),
                request.material(),
                request.priceAdjustment()
        );
    }

    public UpdateInventoryCommand toCommand(UpdateInventoryRequest request) {
        return new UpdateInventoryCommand(request.availableQuantity());
    }

    public CreateProductImageCommand toCommand(CreateProductImageRequest request) {
        return new CreateProductImageCommand(
                request.imageUrl(),
                request.thumbnail(),
                request.sortOrder()
        );
    }

    public UpdateProductImageCommand toCommand(UpdateProductImageRequest request) {
        return new UpdateProductImageCommand(
                request.imageUrl(),
                request.thumbnail(),
                request.sortOrder()
        );
    }

    public ProductVariantResponse toResponse(ProductVariantResult result) {
        return new ProductVariantResponse(
                result.id(),
                result.productId(),
                result.sku(),
                result.size(),
                result.color(),
                result.material(),
                result.priceAdjustment(),
                result.status(),
                result.createdAt(),
                result.updatedAt(),
                toSummaryResponse(result.inventory())
        );
    }

    public ProductInventoryResponse toResponse(ProductInventoryResult result) {
        return new ProductInventoryResponse(
                result.id(),
                result.variantId(),
                result.availableQuantity(),
                result.reservedQuantity(),
                result.soldQuantity(),
                result.updatedAt()
        );
    }

    public ProductImageResponse toResponse(ProductImageResult result) {
        return new ProductImageResponse(
                result.id(),
                result.productId(),
                result.imageUrl(),
                result.thumbnail(),
                result.sortOrder(),
                result.createdAt()
        );
    }

    public ProductDetailResponse toResponse(ProductDetailResult result) {
        return new ProductDetailResponse(
                result.id(),
                result.name(),
                result.description(),
                result.basePrice(),
                result.status(),
                result.createdAt(),
                result.updatedAt(),
                result.images().stream().map(this::toResponse).toList(),
                result.variants().stream().map(this::toResponse).toList()
        );
    }

    public InventorySummaryResponse toSummaryResponse(ProductInventoryResult result) {
        if (result == null) {
            return null;
        }

        return new InventorySummaryResponse(
                result.variantId(),
                result.availableQuantity()
        );
    }
}

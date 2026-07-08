package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.CreateProductImageRequest;
import com.aifashionstudio.catalog.api.dto.ProductImageResponse;
import com.aifashionstudio.catalog.api.dto.UpdateProductImageRequest;
import com.aifashionstudio.catalog.api.mapper.ProductCatalogApiMapper;
import com.aifashionstudio.catalog.application.service.ProductImageApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.UUID;

@RestController
@RequiredArgsConstructor
public class ProductImageController {

    private final ProductImageApplicationService productImageApplicationService;
    private final ProductCatalogApiMapper mapper;

    @GetMapping("/api/products/{productId}/images")
    public ResponseEntity<List<ProductImageResponse>> getPublicImages(@PathVariable UUID productId) {
        return ResponseEntity.ok(
                productImageApplicationService.getPublicImagesByProductId(productId)
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @GetMapping("/admin/catalogs/{productId}/images")
    public ResponseEntity<List<ProductImageResponse>> getImages(@PathVariable UUID productId) {
        return ResponseEntity.ok(
                productImageApplicationService.getImagesByProductId(productId)
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @PostMapping("/admin/catalogs/{productId}/images")
    public ResponseEntity<ProductImageResponse> addImage(
            @PathVariable UUID productId,
            @Valid @RequestBody CreateProductImageRequest request
    ) {
        return ResponseEntity.status(201)
                .body(mapper.toResponse(productImageApplicationService.addImage(productId, mapper.toCommand(request))));
    }

    @PutMapping("/admin/catalogs/{productId}/images/{imageId}")
    public ResponseEntity<ProductImageResponse> updateImage(
            @PathVariable UUID productId,
            @PathVariable UUID imageId,
            @Valid @RequestBody UpdateProductImageRequest request
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(productImageApplicationService.updateImage(productId, imageId, mapper.toCommand(request)))
        );
    }

    @DeleteMapping("/admin/catalogs/{productId}/images/{imageId}")
    public ResponseEntity<Void> deleteImage(
            @PathVariable UUID productId,
            @PathVariable UUID imageId
    ) {
        productImageApplicationService.deleteImage(productId, imageId);
        return ResponseEntity.noContent().build();
    }
}

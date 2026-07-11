package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.ChangeProductVariantStatusRequest;
import com.aifashionstudio.catalog.api.dto.CreateProductVariantRequest;
import com.aifashionstudio.catalog.api.dto.ProductVariantResponse;
import com.aifashionstudio.catalog.api.dto.UpdateProductVariantRequest;
import com.aifashionstudio.catalog.api.mapper.ProductCatalogApiMapper;
import com.aifashionstudio.catalog.application.service.ProductVariantApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PatchMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/admin")
@RequiredArgsConstructor
public class AdminProductVariantController {

    private final ProductVariantApplicationService productVariantApplicationService;
    private final ProductCatalogApiMapper mapper;

    @PostMapping("/catalogs/{productId}/variants")
    public ResponseEntity<ProductVariantResponse> createVariant(
            @PathVariable UUID productId,
            @Valid @RequestBody CreateProductVariantRequest request
    ) {
        return ResponseEntity.status(201)
                .body(mapper.toResponse(
                        productVariantApplicationService.createVariant(productId, mapper.toCommand(request))
                ));
    }

    @GetMapping("/catalogs/{productId}/variants")
    public ResponseEntity<List<ProductVariantResponse>> getVariantsByProduct(@PathVariable UUID productId) {
        return ResponseEntity.ok(
                productVariantApplicationService.getVariantsByProductId(productId)
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @GetMapping("/variants/{id}")
    public ResponseEntity<ProductVariantResponse> getVariantById(@PathVariable UUID id) {
        return ResponseEntity.ok(mapper.toResponse(productVariantApplicationService.getVariantById(id)));
    }

    @PutMapping("/variants/{id}")
    public ResponseEntity<ProductVariantResponse> updateVariant(
            @PathVariable UUID id,
            @Valid @RequestBody UpdateProductVariantRequest request
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(productVariantApplicationService.updateVariant(id, mapper.toCommand(request)))
        );
    }

    @PatchMapping("/variants/{id}/status")
    public ResponseEntity<ProductVariantResponse> changeVariantStatus(
            @PathVariable UUID id,
            @Valid @RequestBody ChangeProductVariantStatusRequest request
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(productVariantApplicationService.changeVariantStatus(id, request.status()))
        );
    }
}

package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.ProductVariantResponse;
import com.aifashionstudio.catalog.api.mapper.ProductCatalogApiMapper;
import com.aifashionstudio.catalog.application.service.ProductVariantApplicationService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api")
@RequiredArgsConstructor
@Tag(name = "Product Variants", description = "Public APIs for selecting product variants.")
public class ProductVariantController {

    private final ProductVariantApplicationService productVariantApplicationService;
    private final ProductCatalogApiMapper mapper;

    @Operation(summary = "List active variants by product")
    @GetMapping("/products/{productId}/variants")
    public ResponseEntity<List<ProductVariantResponse>> getProductVariants(@PathVariable UUID productId) {
        return ResponseEntity.ok(
                productVariantApplicationService.getPublicVariantsByProductId(productId)
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @Operation(summary = "List active variants by product query")
    @GetMapping("/variants")
    public ResponseEntity<List<ProductVariantResponse>> getVariants(@RequestParam UUID productId) {
        return getProductVariants(productId);
    }

    @Operation(summary = "Get active variant by ID")
    @GetMapping("/variants/{id}")
    public ResponseEntity<ProductVariantResponse> getVariantById(@PathVariable UUID id) {
        return ResponseEntity.ok(
                mapper.toResponse(productVariantApplicationService.getPublicVariantById(id))
        );
    }
}

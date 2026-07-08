package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.InventorySummaryResponse;
import com.aifashionstudio.catalog.api.dto.ProductInventoryResponse;
import com.aifashionstudio.catalog.api.dto.UpdateInventoryRequest;
import com.aifashionstudio.catalog.api.mapper.ProductCatalogApiMapper;
import com.aifashionstudio.catalog.application.service.ProductInventoryApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.UUID;

@RestController
@RequiredArgsConstructor
public class ProductInventoryController {

    private final ProductInventoryApplicationService productInventoryApplicationService;
    private final ProductCatalogApiMapper mapper;

    @GetMapping("/api/inventory/{variantId}")
    public ResponseEntity<InventorySummaryResponse> getPublicInventory(@PathVariable UUID variantId) {
        return ResponseEntity.ok(
                mapper.toSummaryResponse(productInventoryApplicationService.getPublicInventoryByVariantId(variantId))
        );
    }

    @GetMapping("/admin/inventory/{variantId}")
    public ResponseEntity<ProductInventoryResponse> getInventory(@PathVariable UUID variantId) {
        return ResponseEntity.ok(
                mapper.toResponse(productInventoryApplicationService.getInventoryByVariantId(variantId))
        );
    }

    @PutMapping("/admin/inventory/{variantId}")
    public ResponseEntity<ProductInventoryResponse> updateInventory(
            @PathVariable UUID variantId,
            @Valid @RequestBody UpdateInventoryRequest request
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(productInventoryApplicationService.updateInventory(variantId, mapper.toCommand(request)))
        );
    }
}

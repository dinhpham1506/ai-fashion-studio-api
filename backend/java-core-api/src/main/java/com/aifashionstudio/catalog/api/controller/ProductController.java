package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.CatalogResponse;
import com.aifashionstudio.catalog.api.mapper.CatalogApiMapper;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
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
@RequestMapping("/api/products")
@RequiredArgsConstructor
public class ProductController {
    private final CatalogApplicationService catalogApplicationService;
    private final CatalogApiMapper mapper;

    @GetMapping
    public ResponseEntity<List<CatalogResponse>> getProducts(
            @RequestParam(required = false) String name
    ) {
        return ResponseEntity.ok(
                catalogApplicationService.getPublicProducts(name)
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @GetMapping("/{id}")
    public ResponseEntity<CatalogResponse> getProductById(@PathVariable UUID id) {
        return ResponseEntity.ok(
                mapper.toResponse(catalogApplicationService.getPublicProductById(id))
        );
    }
}

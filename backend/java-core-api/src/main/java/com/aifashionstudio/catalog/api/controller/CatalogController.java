package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.CatalogResponse;
import com.aifashionstudio.catalog.api.dto.CreateCatalogRequest;
import com.aifashionstudio.catalog.api.mapper.CatalogApiMapper;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/admin/catalogs")
@RequiredArgsConstructor
public class CatalogController {
    private final CatalogApplicationService catalogApplicationService;
    private final CatalogApiMapper mapper;
    @PostMapping("/create")
    public ResponseEntity<CatalogResponse> createCatalog
            (@Valid  @RequestBody CreateCatalogRequest request) {
        CatalogResult result = catalogApplicationService.createCatalog(mapper.toCommand(request));

        return ResponseEntity.status(201)
                .body(mapper.toResponse(result));
    }
    @GetMapping
    public ResponseEntity<List<CatalogResponse>> getCatalogs() {
        return ResponseEntity.ok(
                catalogApplicationService.getCatalogs()
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @GetMapping("/{id}")
    public ResponseEntity<CatalogResponse> getCatalogById(@PathVariable UUID id) {
        return ResponseEntity.ok(
                mapper.toResponse(catalogApplicationService.getCatalogById(id))
        );
    }



}

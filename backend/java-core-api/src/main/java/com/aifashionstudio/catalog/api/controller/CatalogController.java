package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.CatalogResponse;
import com.aifashionstudio.catalog.api.dto.ChangeCatalogStatusRequest;
import com.aifashionstudio.catalog.api.dto.CreateCatalogRequest;
import com.aifashionstudio.catalog.api.dto.UpdateCatalogRequest;
import com.aifashionstudio.catalog.api.mapper.CatalogApiMapper;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
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
    public ResponseEntity<List<CatalogResponse>> getCatalogs(
            @RequestParam(required = false) CatalogStatus status,
            @RequestParam(required = false) String name
    ) {
        return ResponseEntity.ok(
                catalogApplicationService.getCatalogs(status, name)
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

    @PutMapping("/{id}")
    public ResponseEntity<CatalogResponse> updateCatalog(
            @PathVariable UUID id,
            @Valid @RequestBody UpdateCatalogRequest request
    ) {
        CatalogResult result = catalogApplicationService.updateCatalog(mapper.toCommand(id, request));
        return ResponseEntity.ok(mapper.toResponse(result));
    }

    @PatchMapping("/{id}/status")
    public ResponseEntity<CatalogResponse> changeCatalogStatus(
            @PathVariable UUID id,
            @Valid @RequestBody ChangeCatalogStatusRequest request
    ) {
        CatalogResult result = catalogApplicationService.changeCatalogStatus(mapper.toCommand(id, request));
        return ResponseEntity.ok(mapper.toResponse(result));
    }

}

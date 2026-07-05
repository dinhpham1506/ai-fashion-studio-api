package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.CatalogResponse;
import com.aifashionstudio.catalog.api.dto.ChangeCatalogStatusRequest;
import com.aifashionstudio.catalog.api.dto.CreateCatalogRequest;
import com.aifashionstudio.catalog.api.dto.UpdateCatalogRequest;
import com.aifashionstudio.catalog.api.mapper.CatalogApiMapper;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.Parameter;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.Schema;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/admin/catalogs")
@RequiredArgsConstructor
@Tag(name = "Admin Catalogs", description = "Admin APIs for creating, updating, searching, and publishing catalog products.")
public class CatalogController {
    private final CatalogApplicationService catalogApplicationService;
    private final CatalogApiMapper mapper;

    @Operation(
            summary = "Create catalog product",
            description = "Creates a new catalog product in DRAFT status. Product name must be unique.",
            responses = {
                    @ApiResponse(responseCode = "201", description = "Catalog product created",
                            content = @Content(schema = @Schema(implementation = CatalogResponse.class))),
                    @ApiResponse(responseCode = "409", description = "Catalog name already exists"),
                    @ApiResponse(responseCode = "422", description = "Business validation failed")
            }
    )
    @PostMapping("/create")
    public ResponseEntity<CatalogResponse> createCatalog
            (@Valid  @RequestBody CreateCatalogRequest request) {
        CatalogResult result = catalogApplicationService.createCatalog(mapper.toCommand(request));

        return ResponseEntity.status(201)
                .body(mapper.toResponse(result));
    }

    @Operation(
            summary = "List admin catalog products",
            description = "Returns catalog products for admin management. Optional filters support status and partial product name.",
            responses = {
                    @ApiResponse(responseCode = "200", description = "Catalog products returned")
            }
    )
    @GetMapping
    public ResponseEntity<List<CatalogResponse>> getCatalogs(
            @Parameter(description = "Filter by catalog product status")
            @RequestParam(required = false) CatalogStatus status,
            @Parameter(description = "Filter by partial product name, case-insensitive")
            @RequestParam(required = false) String name
    ) {
        return ResponseEntity.ok(
                catalogApplicationService.getCatalogs(status, name)
                        .stream()
                        .map(mapper::toResponse)
                .toList()
        );
    }

    @Operation(
            summary = "Get admin catalog product by ID",
            description = "Returns one catalog product by ID for admin management.",
            responses = {
                    @ApiResponse(responseCode = "200", description = "Catalog product returned",
                            content = @Content(schema = @Schema(implementation = CatalogResponse.class))),
                    @ApiResponse(responseCode = "404", description = "Catalog product not found")
            }
    )
    @GetMapping("/{id}")
    public ResponseEntity<CatalogResponse> getCatalogById(
            @Parameter(description = "Catalog product ID") @PathVariable UUID id
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(catalogApplicationService.getCatalogById(id))
        );
    }

    @Operation(
            summary = "Update catalog product",
            description = "Updates name, description, and base price. Archived products cannot be updated.",
            responses = {
                    @ApiResponse(responseCode = "200", description = "Catalog product updated",
                            content = @Content(schema = @Schema(implementation = CatalogResponse.class))),
                    @ApiResponse(responseCode = "404", description = "Catalog product not found"),
                    @ApiResponse(responseCode = "409", description = "Catalog name already exists"),
                    @ApiResponse(responseCode = "422", description = "Catalog cannot be updated")
            }
    )
    @PutMapping("/{id}")
    public ResponseEntity<CatalogResponse> updateCatalog(
            @Parameter(description = "Catalog product ID") @PathVariable UUID id,
            @Valid @RequestBody UpdateCatalogRequest request
    ) {
        CatalogResult result = catalogApplicationService.updateCatalog(mapper.toCommand(id, request));
        return ResponseEntity.ok(mapper.toResponse(result));
    }

    @Operation(
            summary = "Change catalog product status",
            description = "Changes catalog product status. A product cannot move directly from DRAFT to ARCHIVED.",
            responses = {
                    @ApiResponse(responseCode = "200", description = "Catalog status changed",
                            content = @Content(schema = @Schema(implementation = CatalogResponse.class))),
                    @ApiResponse(responseCode = "404", description = "Catalog product not found"),
                    @ApiResponse(responseCode = "422", description = "Invalid status transition")
            }
    )
    @PatchMapping("/{id}/status")
    public ResponseEntity<CatalogResponse> changeCatalogStatus(
            @Parameter(description = "Catalog product ID") @PathVariable UUID id,
            @Valid @RequestBody ChangeCatalogStatusRequest request
    ) {
        CatalogResult result = catalogApplicationService.changeCatalogStatus(mapper.toCommand(id, request));
        return ResponseEntity.ok(mapper.toResponse(result));
    }

}

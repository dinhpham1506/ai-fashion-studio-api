package com.aifashionstudio.catalog.api.controller;

import com.aifashionstudio.catalog.api.dto.CatalogResponse;
import com.aifashionstudio.catalog.api.mapper.CatalogApiMapper;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.Parameter;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.Schema;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
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
@RequestMapping("/api/products")
@RequiredArgsConstructor
@Tag(name = "Products", description = "Public product APIs for the frontend. Only ACTIVE catalog products are returned.")
public class ProductController {
    private final CatalogApplicationService catalogApplicationService;
    private final CatalogApiMapper mapper;

    @Operation(
            summary = "List public products",
            description = "Returns ACTIVE catalog products for the frontend. Optional name filter supports case-insensitive search.",
            responses = {
                    @ApiResponse(responseCode = "200", description = "Public products returned")
            }
    )
    @GetMapping
    public ResponseEntity<List<CatalogResponse>> getProducts(
            @Parameter(description = "Filter by partial product name, case-insensitive")
            @RequestParam(required = false) String name
    ) {
        return ResponseEntity.ok(
                catalogApplicationService.getPublicProducts(name)
                        .stream()
                        .map(mapper::toResponse)
                        .toList()
        );
    }

    @Operation(
            summary = "Get public product by ID",
            description = "Returns an ACTIVE catalog product by ID. Inactive, draft, and archived products are hidden from public clients.",
            responses = {
                    @ApiResponse(responseCode = "200", description = "Public product returned",
                            content = @Content(schema = @Schema(implementation = CatalogResponse.class))),
                    @ApiResponse(responseCode = "404", description = "Product not found or not public")
            }
    )
    @GetMapping("/{id}")
    public ResponseEntity<CatalogResponse> getProductById(
            @Parameter(description = "Product ID") @PathVariable UUID id
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(catalogApplicationService.getPublicProductById(id))
        );
    }
}

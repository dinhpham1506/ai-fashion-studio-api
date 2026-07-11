package com.aifashionstudio.design.api.controller;

import com.aifashionstudio.design.api.dto.CreateDraftDesignRequest;
import com.aifashionstudio.design.api.dto.DesignDetailResponse;
import com.aifashionstudio.design.api.dto.DesignDraftResponse;
import com.aifashionstudio.design.api.dto.DesignSavedResponse;
import com.aifashionstudio.design.api.dto.PagedDesignResponse;
import com.aifashionstudio.design.api.dto.SaveDesignRequest;
import com.aifashionstudio.design.api.mapper.DesignApiMapper;
import com.aifashionstudio.design.application.service.DesignApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.UUID;

@RestController
@RequestMapping("/api/designs")
@RequiredArgsConstructor
public class DesignController {

    private final DesignApplicationService designApplicationService;
    private final DesignApiMapper mapper;

    @PostMapping
    public ResponseEntity<DesignDraftResponse> createDraft(
            @RequestHeader("X-User-Id") UUID customerId,
            @Valid @RequestBody CreateDraftDesignRequest request
    ) {
        return ResponseEntity.status(201)
                .body(mapper.toResponse(designApplicationService.createDraft(mapper.toCommand(customerId, request))));
    }

    @PutMapping("/{designId}/save")
    public ResponseEntity<DesignSavedResponse> saveDesign(
            @RequestHeader("X-User-Id") UUID customerId,
            @PathVariable UUID designId,
            @Valid @RequestBody SaveDesignRequest request
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(designApplicationService.saveDesign(mapper.toCommand(customerId, designId, request)))
        );
    }

    @GetMapping("/my")
    public ResponseEntity<PagedDesignResponse> getMyDesigns(
            @RequestHeader("X-User-Id") UUID customerId,
            @RequestParam(defaultValue = "1") int page,
            @RequestParam(defaultValue = "10") int pageSize
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(designApplicationService.getMyDesigns(customerId, page, pageSize))
        );
    }

    @GetMapping("/{designId}")
    public ResponseEntity<DesignDetailResponse> getDesignDetail(
            @RequestHeader("X-User-Id") UUID customerId,
            @PathVariable UUID designId
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(designApplicationService.getDesignDetail(customerId, designId))
        );
    }
}

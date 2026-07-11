package com.aifashionstudio.design.api.dto;

import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.util.List;
import java.util.Map;

public record SaveDesignRequest(
        @NotBlank
        String name,

        @NotNull
        Map<String, Object> canvasJson,

        String previewImageUrl,

        String printFileUrl,

        @Valid
        List<SaveDesignLayerRequest> layers
) {
}

package com.aifashionstudio.design.api.dto;

import java.util.UUID;

public record DesignDraftResponse(
        UUID designId,
        String status
) {
}

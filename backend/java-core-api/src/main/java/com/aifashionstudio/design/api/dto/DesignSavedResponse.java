package com.aifashionstudio.design.api.dto;

import java.util.UUID;

public record DesignSavedResponse(
        UUID designId,
        String status,
        String previewImageUrl,
        String printFileUrl
) {
}

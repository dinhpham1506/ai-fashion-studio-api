package com.aifashionstudio.design.application.dto;

import com.aifashionstudio.design.domain.model.DesignStatus;

import java.util.UUID;

public record DesignSavedResult(
        UUID designId,
        DesignStatus status,
        String previewImageUrl,
        String printFileUrl
) {
}

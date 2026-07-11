package com.aifashionstudio.design.application.dto;

import com.aifashionstudio.design.domain.model.DesignStatus;

import java.util.UUID;

public record DesignDraftResult(
        UUID designId,
        DesignStatus status
) {
}

package com.aifashionstudio.design.application.command;

import java.util.UUID;

public record CreateDraftDesignCommand(
        UUID customerId,
        UUID productId,
        UUID productVariantId,
        String name
) {
}

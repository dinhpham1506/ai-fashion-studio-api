package com.aifashionstudio.ordering.application.command;

import java.util.UUID;

public record AddCartItemCommand(
        UUID customerId,
        UUID productId,
        UUID productVariantId,
        UUID designId,
        int quantity
) {
}

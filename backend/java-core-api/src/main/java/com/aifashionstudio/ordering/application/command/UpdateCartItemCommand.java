package com.aifashionstudio.ordering.application.command;

import java.util.UUID;

public record UpdateCartItemCommand(
        UUID customerId,
        UUID cartItemId,
        int quantity
) {
}

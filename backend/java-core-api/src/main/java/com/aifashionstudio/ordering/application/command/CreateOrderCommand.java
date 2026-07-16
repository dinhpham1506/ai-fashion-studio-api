package com.aifashionstudio.ordering.application.command;

import java.util.List;
import java.util.UUID;

public record CreateOrderCommand(
        UUID customerId,
        List<CreateOrderItemCommand> items,
        String receiverName,
        String receiverPhone,
        String shippingAddress
) {
    public record CreateOrderItemCommand(
            UUID productId,
            UUID productVariantId,
            UUID designId,
            int quantity
    ) {
    }
}

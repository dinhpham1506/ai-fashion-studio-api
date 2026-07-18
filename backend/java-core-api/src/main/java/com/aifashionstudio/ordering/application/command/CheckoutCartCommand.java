package com.aifashionstudio.ordering.application.command;

import java.util.UUID;

public record CheckoutCartCommand(
        UUID customerId,
        String receiverName,
        String receiverPhone,
        String shippingAddress
) {
}

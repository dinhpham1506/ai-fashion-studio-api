package com.aifashionstudio.ordering.api.dto;

import jakarta.validation.constraints.NotBlank;

public record CheckoutCartRequest(
        @NotBlank
        String receiverName,

        @NotBlank
        String receiverPhone,

        @NotBlank
        String shippingAddress
) {
}

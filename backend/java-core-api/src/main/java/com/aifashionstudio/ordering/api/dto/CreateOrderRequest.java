package com.aifashionstudio.ordering.api.dto;

import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotEmpty;

import java.util.List;

public record CreateOrderRequest(
        @NotEmpty
        @Valid
        List<CreateOrderItemRequest> items,

        @NotBlank
        String receiverName,

        @NotBlank
        String receiverPhone,

        @NotBlank
        String shippingAddress
) {
}

package com.aifashionstudio.ordering.api.dto;

import jakarta.validation.constraints.Positive;

public record UpdateCartItemRequest(
        @Positive
        int quantity
) {
}

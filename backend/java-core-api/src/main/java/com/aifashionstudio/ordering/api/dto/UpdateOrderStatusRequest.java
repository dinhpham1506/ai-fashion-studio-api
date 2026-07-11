package com.aifashionstudio.ordering.api.dto;

import com.aifashionstudio.ordering.domain.model.OrderStatus;
import jakarta.validation.constraints.NotNull;

public record UpdateOrderStatusRequest(
        @NotNull
        OrderStatus toStatus,

        String note
) {
}

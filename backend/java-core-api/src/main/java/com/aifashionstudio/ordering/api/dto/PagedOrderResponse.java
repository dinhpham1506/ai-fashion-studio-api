package com.aifashionstudio.ordering.api.dto;

import java.util.List;

public record PagedOrderResponse(
        List<OrderSummaryResponse> items,
        int page,
        int pageSize,
        long totalItems,
        int totalPages
) {
}

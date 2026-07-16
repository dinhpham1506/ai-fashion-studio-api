package com.aifashionstudio.ordering.application.dto;

import java.util.List;

public record PagedOrderResult(
        List<OrderSummaryResult> items,
        int page,
        int pageSize,
        long totalItems,
        int totalPages
) {
}

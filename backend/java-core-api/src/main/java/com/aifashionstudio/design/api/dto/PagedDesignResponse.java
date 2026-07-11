package com.aifashionstudio.design.api.dto;

import java.util.List;

public record PagedDesignResponse(
        List<DesignSummaryResponse> items,
        int page,
        int pageSize,
        long totalItems,
        int totalPages
) {
}

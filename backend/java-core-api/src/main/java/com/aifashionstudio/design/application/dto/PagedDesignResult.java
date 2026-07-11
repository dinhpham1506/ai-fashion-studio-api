package com.aifashionstudio.design.application.dto;

import java.util.List;

public record PagedDesignResult(
        List<DesignSummaryResult> items,
        int page,
        int pageSize,
        long totalItems,
        int totalPages
) {
}

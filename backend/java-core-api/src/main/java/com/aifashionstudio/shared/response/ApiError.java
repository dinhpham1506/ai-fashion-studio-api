package com.aifashionstudio.shared.response;

public record ApiError(
    String field,
    String code,
    String message
) {
}

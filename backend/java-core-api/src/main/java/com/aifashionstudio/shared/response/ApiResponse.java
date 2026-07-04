
package com.aifashionstudio.shared.response;

import java.util.List;

public record ApiResponse(
        boolean success,
        String message,
        Object data,
        List<ApiError> errors,
        ResponseMeta meta
) {
    public static ApiResponse success(String message, Object data) {
        return new ApiResponse(
                true,
                message,
                data,
                null,
                ResponseMeta.now()
        );
    }

    public static ApiResponse error(String message, List<ApiError>
            errors) {
        return new ApiResponse(
                false,
                message,
                null,
                errors,
                ResponseMeta.now()
        );
    }
}

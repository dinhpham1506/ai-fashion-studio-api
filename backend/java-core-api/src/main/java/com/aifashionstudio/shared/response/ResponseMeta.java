package com.aifashionstudio.shared.response;

import java.time.OffsetDateTime;
import java.util.UUID;

public record ResponseMeta(
        String requestId,
        OffsetDateTime timestamp
) {
    public static ResponseMeta now() {
        return new ResponseMeta(
                "req_" + UUID.randomUUID(),
                OffsetDateTime.now()
        );
    }
}

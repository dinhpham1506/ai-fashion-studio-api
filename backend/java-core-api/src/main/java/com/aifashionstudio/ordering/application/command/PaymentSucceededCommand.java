package com.aifashionstudio.ordering.application.command;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

public record PaymentSucceededCommand(
        UUID paymentId,
        UUID orderId,
        UUID customerId,
        BigDecimal amount,
        String provider,
        String transactionCode,
        OffsetDateTime paidAt,
        String invoiceNumber,
        String invoicePdfUrl
) {
}

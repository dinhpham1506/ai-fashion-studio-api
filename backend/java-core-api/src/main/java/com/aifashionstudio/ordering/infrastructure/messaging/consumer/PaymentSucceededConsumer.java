package com.aifashionstudio.ordering.infrastructure.messaging.consumer;

import com.aifashionstudio.ordering.application.command.PaymentSucceededCommand;
import com.aifashionstudio.ordering.application.service.OrderApplicationService;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.stereotype.Component;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.UUID;

@Slf4j
@Component
@RequiredArgsConstructor
public class PaymentSucceededConsumer {

    private final ObjectMapper objectMapper;
    private final OrderApplicationService orderApplicationService;

    @KafkaListener(topics = "payment.events", groupId = "java-core-api")
    public void onMessage(String payload) {
        try {
            JsonNode root = objectMapper.readTree(payload);
            if (!"PaymentSucceeded".equals(root.path("eventType").asText())) {
                return;
            }

            JsonNode data = root.has("data") ? root.path("data") : root;
            orderApplicationService.handlePaymentSucceeded(new PaymentSucceededCommand(
                    uuid(data, "paymentId"),
                    uuid(data, "orderId"),
                    uuid(data, "customerId"),
                    decimal(data, "amount"),
                    text(data, "provider", text(data, "paymentMethod", null)),
                    text(data, "transactionCode", null),
                    dateTime(data, "paidAt", root.path("occurredAt").asText(null)),
                    text(data, "invoiceNumber", null),
                    text(data, "invoicePdfUrl", null)
            ));
        } catch (Exception ex) {
            log.error("Failed to consume PaymentSucceeded event", ex);
            throw new IllegalStateException("Failed to consume PaymentSucceeded event", ex);
        }
    }

    private UUID uuid(JsonNode node, String field) {
        return UUID.fromString(node.path(field).asText());
    }

    private BigDecimal decimal(JsonNode node, String field) {
        return node.path(field).decimalValue();
    }

    private String text(JsonNode node, String field, String defaultValue) {
        JsonNode value = node.get(field);
        return value == null || value.isNull() ? defaultValue : value.asText();
    }

    private OffsetDateTime dateTime(JsonNode node, String field, String defaultValue) {
        String value = text(node, field, defaultValue);
        return value == null ? null : OffsetDateTime.parse(value);
    }
}

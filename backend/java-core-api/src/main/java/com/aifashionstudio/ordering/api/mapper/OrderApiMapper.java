package com.aifashionstudio.ordering.api.mapper;

import com.aifashionstudio.ordering.api.dto.CreateOrderRequest;
import com.aifashionstudio.ordering.api.dto.OrderCreatedResponse;
import com.aifashionstudio.ordering.api.dto.OrderDetailResponse;
import com.aifashionstudio.ordering.api.dto.OrderItemResponse;
import com.aifashionstudio.ordering.api.dto.OrderStatusHistoryResponse;
import com.aifashionstudio.ordering.api.dto.OrderStatusUpdatedResponse;
import com.aifashionstudio.ordering.api.dto.OrderSummaryResponse;
import com.aifashionstudio.ordering.api.dto.PagedOrderResponse;
import com.aifashionstudio.ordering.application.command.CreateOrderCommand;
import com.aifashionstudio.ordering.application.dto.OrderCreatedResult;
import com.aifashionstudio.ordering.application.dto.OrderDetailResult;
import com.aifashionstudio.ordering.application.dto.OrderItemResult;
import com.aifashionstudio.ordering.application.dto.OrderStatusHistoryResult;
import com.aifashionstudio.ordering.application.dto.OrderStatusUpdatedResult;
import com.aifashionstudio.ordering.application.dto.OrderSummaryResult;
import com.aifashionstudio.ordering.application.dto.PagedOrderResult;
import org.springframework.stereotype.Component;

import java.util.UUID;

@Component
public class OrderApiMapper {

    public CreateOrderCommand toCommand(UUID customerId, CreateOrderRequest request) {
        return new CreateOrderCommand(
                customerId,
                request.items().stream()
                        .map(item -> new CreateOrderCommand.CreateOrderItemCommand(
                                item.productId(),
                                item.productVariantId(),
                                item.designId(),
                                item.quantity()
                        ))
                        .toList(),
                request.receiverName(),
                request.receiverPhone(),
                request.shippingAddress()
        );
    }

    public OrderCreatedResponse toResponse(OrderCreatedResult result) {
        return new OrderCreatedResponse(
                result.orderId(),
                result.orderCode(),
                result.totalAmount(),
                result.paymentStatus().name(),
                result.orderStatus().name()
        );
    }

    public PagedOrderResponse toResponse(PagedOrderResult result) {
        return new PagedOrderResponse(
                result.items().stream().map(this::toResponse).toList(),
                result.page(),
                result.pageSize(),
                result.totalItems(),
                result.totalPages()
        );
    }

    public OrderDetailResponse toResponse(OrderDetailResult result) {
        return new OrderDetailResponse(
                result.id(),
                result.orderCode(),
                result.customerId(),
                result.totalAmount(),
                result.paymentStatus().name(),
                result.orderStatus().name(),
                result.receiverName(),
                result.receiverPhone(),
                result.shippingAddress(),
                result.items().stream().map(this::toResponse).toList(),
                result.statusHistory().stream().map(this::toResponse).toList()
        );
    }

    public OrderStatusUpdatedResponse toResponse(OrderStatusUpdatedResult result) {
        return new OrderStatusUpdatedResponse(
                result.orderId(),
                result.fromStatus().name(),
                result.toStatus().name()
        );
    }

    private OrderSummaryResponse toResponse(OrderSummaryResult result) {
        return new OrderSummaryResponse(
                result.id(),
                result.orderCode(),
                result.totalAmount(),
                result.paymentStatus().name(),
                result.orderStatus().name(),
                result.createdAt()
        );
    }

    private OrderItemResponse toResponse(OrderItemResult result) {
        return new OrderItemResponse(
                result.id(),
                result.productId(),
                result.productVariantId(),
                result.designId(),
                result.productNameSnapshot(),
                result.variantSnapshot(),
                result.quantity(),
                result.unitPrice(),
                result.totalPrice()
        );
    }

    private OrderStatusHistoryResponse toResponse(OrderStatusHistoryResult result) {
        return new OrderStatusHistoryResponse(
                result.id(),
                result.fromStatus() == null ? null : result.fromStatus().name(),
                result.toStatus().name(),
                result.changedBy(),
                result.note(),
                result.createdAt()
        );
    }
}

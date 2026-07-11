package com.aifashionstudio.ordering.application.mapper;

import com.aifashionstudio.ordering.application.dto.OrderDetailResult;
import com.aifashionstudio.ordering.application.dto.OrderCreatedResult;
import com.aifashionstudio.ordering.application.dto.OrderItemResult;
import com.aifashionstudio.ordering.application.dto.OrderStatusHistoryResult;
import com.aifashionstudio.ordering.application.dto.OrderSummaryResult;
import com.aifashionstudio.ordering.domain.model.Order;
import com.aifashionstudio.ordering.domain.model.OrderItem;
import com.aifashionstudio.ordering.domain.model.OrderStatusHistory;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class OrderApplicationMapper {

    public OrderCreatedResult toCreatedResult(Order order) {
        return new OrderCreatedResult(
                order.getId(),
                order.getOrderCode(),
                order.getTotalAmount(),
                order.getPaymentStatus(),
                order.getOrderStatus()
        );
    }

    public OrderSummaryResult toSummaryResult(Order order) {
        return new OrderSummaryResult(
                order.getId(),
                order.getOrderCode(),
                order.getTotalAmount(),
                order.getPaymentStatus(),
                order.getOrderStatus(),
                order.getCreatedAt()
        );
    }

    public OrderDetailResult toDetailResult(Order order, List<OrderStatusHistory> histories) {
        return new OrderDetailResult(
                order.getId(),
                order.getOrderCode(),
                order.getCustomerId(),
                order.getTotalAmount(),
                order.getPaymentStatus(),
                order.getOrderStatus(),
                order.getReceiverName(),
                order.getReceiverPhone(),
                order.getShippingAddress(),
                order.getItems().stream().map(this::toItemResult).toList(),
                histories.stream().map(this::toHistoryResult).toList()
        );
    }

    private OrderItemResult toItemResult(OrderItem item) {
        return new OrderItemResult(
                item.getId(),
                item.getProductId(),
                item.getProductVariantId(),
                item.getDesignId(),
                item.getProductNameSnapshot(),
                item.getVariantSnapshot(),
                item.getQuantity(),
                item.getUnitPrice(),
                item.getTotalPrice()
        );
    }

    private OrderStatusHistoryResult toHistoryResult(OrderStatusHistory history) {
        return new OrderStatusHistoryResult(
                history.getId(),
                history.getFromStatus(),
                history.getToStatus(),
                history.getChangedBy(),
                history.getNote(),
                history.getCreatedAt()
        );
    }
}

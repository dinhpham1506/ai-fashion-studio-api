package com.aifashionstudio.ordering.application.service;

import com.aifashionstudio.ordering.application.command.CreateOrderCommand;
import com.aifashionstudio.ordering.application.command.PaymentSucceededCommand;
import com.aifashionstudio.ordering.application.command.UpdateOrderStatusCommand;
import com.aifashionstudio.ordering.application.dto.OrderCreatedResult;
import com.aifashionstudio.ordering.application.dto.OrderDetailResult;
import com.aifashionstudio.ordering.application.dto.OrderStatusUpdatedResult;
import com.aifashionstudio.ordering.application.dto.PagedOrderResult;

public interface OrderApplicationService {

    OrderCreatedResult createOrder(CreateOrderCommand command);

    PagedOrderResult getMyOrders(java.util.UUID customerId, int page, int pageSize);

    OrderDetailResult getOrderDetail(java.util.UUID requesterId, boolean staffOrAdmin, java.util.UUID orderId);

    OrderStatusUpdatedResult updateOrderStatus(UpdateOrderStatusCommand command);

    void handlePaymentSucceeded(PaymentSucceededCommand command);
}

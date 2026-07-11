package com.aifashionstudio.ordering.domain.repository;

import com.aifashionstudio.ordering.domain.model.OrderStatusHistory;

import java.util.List;
import java.util.UUID;

public interface OrderStatusHistoryRepository {

    OrderStatusHistory save(OrderStatusHistory history);

    List<OrderStatusHistory> findByOrderId(UUID orderId);
}

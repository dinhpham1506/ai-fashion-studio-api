package com.aifashionstudio.ordering.domain.repository;

import com.aifashionstudio.ordering.domain.model.Order;
import com.aifashionstudio.ordering.domain.model.OrderStatus;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface OrderRepository {

    Order save(Order order);

    Optional<Order> findById(UUID id);

    List<Order> findByCustomerId(UUID customerId, int page, int pageSize);

    long countByCustomerId(UUID customerId);

    List<Order> findByOrderStatus(OrderStatus status, int page, int pageSize);

    long countByOrderStatus(OrderStatus status);
}

package com.aifashionstudio.ordering.infrastructure.persistence.adapter;

import com.aifashionstudio.ordering.domain.model.Order;
import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.domain.repository.OrderRepository;
import com.aifashionstudio.ordering.infrastructure.persistence.mapper.OrderPersistenceMapper;
import com.aifashionstudio.ordering.infrastructure.persistence.repository.JpaOrderRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class OrderPersistenceAdapter implements OrderRepository {

    private final JpaOrderRepository jpaOrderRepository;
    private final OrderPersistenceMapper mapper;

    @Override
    public Order save(Order order) {
        return mapper.toDomain(jpaOrderRepository.save(mapper.toEntity(order)));
    }

    @Override
    public Optional<Order> findById(UUID id) {
        return jpaOrderRepository.findById(id)
                .map(mapper::toDomain);
    }

    @Override
    public List<Order> findByCustomerId(UUID customerId, int page, int pageSize) {
        return jpaOrderRepository.findByCustomerIdOrderByCreatedAtDesc(customerId, PageRequest.of(page - 1, pageSize))
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public long countByCustomerId(UUID customerId) {
        return jpaOrderRepository.countByCustomerId(customerId);
    }

    @Override
    public List<Order> findByOrderStatus(OrderStatus status, int page, int pageSize) {
        return jpaOrderRepository.findByOrderStatusOrderByCreatedAtAsc(status, PageRequest.of(page - 1, pageSize))
                .stream()
                .map(mapper::toDomain)
                .toList();
    }

    @Override
    public long countByOrderStatus(OrderStatus status) {
        return jpaOrderRepository.countByOrderStatus(status);
    }
}

package com.aifashionstudio.ordering.infrastructure.persistence.adapter;

import com.aifashionstudio.ordering.domain.model.OrderStatusHistory;
import com.aifashionstudio.ordering.domain.repository.OrderStatusHistoryRepository;
import com.aifashionstudio.ordering.infrastructure.persistence.mapper.OrderPersistenceMapper;
import com.aifashionstudio.ordering.infrastructure.persistence.repository.JpaOrderStatusHistoryRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class OrderStatusHistoryPersistenceAdapter implements OrderStatusHistoryRepository {

    private final JpaOrderStatusHistoryRepository jpaOrderStatusHistoryRepository;
    private final OrderPersistenceMapper mapper;

    @Override
    public OrderStatusHistory save(OrderStatusHistory history) {
        return mapper.toDomain(jpaOrderStatusHistoryRepository.save(mapper.toEntity(history)));
    }

    @Override
    public List<OrderStatusHistory> findByOrderId(UUID orderId) {
        return jpaOrderStatusHistoryRepository.findByOrderIdOrderByCreatedAtAsc(orderId)
                .stream()
                .map(mapper::toDomain)
                .toList();
    }
}

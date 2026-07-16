package com.aifashionstudio.ordering.infrastructure.persistence.adapter;

import com.aifashionstudio.ordering.domain.model.Cart;
import com.aifashionstudio.ordering.domain.repository.CartRepository;
import com.aifashionstudio.ordering.infrastructure.persistence.mapper.CartPersistenceMapper;
import com.aifashionstudio.ordering.infrastructure.persistence.repository.JpaCartRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.Optional;
import java.util.UUID;

@Repository
@RequiredArgsConstructor
public class CartPersistenceAdapter implements CartRepository {

    private final JpaCartRepository jpaCartRepository;
    private final CartPersistenceMapper mapper;

    @Override
    public Cart save(Cart cart) {
        return mapper.toDomain(jpaCartRepository.save(mapper.toEntity(cart)));
    }

    @Override
    public Optional<Cart> findByCustomerId(UUID customerId) {
        return jpaCartRepository.findByCustomerId(customerId)
                .map(mapper::toDomain);
    }

    @Override
    public void deleteByCustomerId(UUID customerId) {
        jpaCartRepository.deleteByCustomerId(customerId);
    }
}

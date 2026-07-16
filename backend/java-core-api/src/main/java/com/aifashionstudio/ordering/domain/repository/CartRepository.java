package com.aifashionstudio.ordering.domain.repository;

import com.aifashionstudio.ordering.domain.model.Cart;

import java.util.Optional;
import java.util.UUID;

public interface CartRepository {

    Cart save(Cart cart);

    Optional<Cart> findByCustomerId(UUID customerId);

    void deleteByCustomerId(UUID customerId);
}

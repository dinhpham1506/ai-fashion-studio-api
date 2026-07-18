package com.aifashionstudio.ordering.application.service;

import com.aifashionstudio.ordering.application.command.AddCartItemCommand;
import com.aifashionstudio.ordering.application.command.CheckoutCartCommand;
import com.aifashionstudio.ordering.application.command.UpdateCartItemCommand;
import com.aifashionstudio.ordering.application.dto.CartResult;
import com.aifashionstudio.ordering.application.dto.OrderCreatedResult;

import java.util.UUID;

public interface CartApplicationService {

    CartResult getCart(UUID customerId);

    CartResult addItem(AddCartItemCommand command);

    CartResult updateItem(UpdateCartItemCommand command);

    CartResult removeItem(UUID customerId, UUID cartItemId);

    CartResult clearCart(UUID customerId);

    OrderCreatedResult checkout(CheckoutCartCommand command);
}

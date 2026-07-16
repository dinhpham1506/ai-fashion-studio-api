package com.aifashionstudio.ordering.api.controller;

import com.aifashionstudio.ordering.api.dto.AddCartItemRequest;
import com.aifashionstudio.ordering.api.dto.CartResponse;
import com.aifashionstudio.ordering.api.dto.CheckoutCartRequest;
import com.aifashionstudio.ordering.api.dto.OrderCreatedResponse;
import com.aifashionstudio.ordering.api.dto.UpdateCartItemRequest;
import com.aifashionstudio.ordering.api.mapper.CartApiMapper;
import com.aifashionstudio.ordering.api.mapper.OrderApiMapper;
import com.aifashionstudio.ordering.application.service.CartApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PatchMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.UUID;

@RestController
@RequestMapping("/api/cart")
@RequiredArgsConstructor
public class CartController {

    private final CartApplicationService cartApplicationService;
    private final CartApiMapper mapper;
    private final OrderApiMapper orderApiMapper;

    @GetMapping
    public ResponseEntity<CartResponse> getCart(
            @RequestHeader("X-User-Id") UUID customerId
    ) {
        return ResponseEntity.ok(mapper.toResponse(cartApplicationService.getCart(customerId)));
    }

    @PostMapping("/items")
    public ResponseEntity<CartResponse> addItem(
            @RequestHeader("X-User-Id") UUID customerId,
            @Valid @RequestBody AddCartItemRequest request
    ) {
        return ResponseEntity.status(201)
                .body(mapper.toResponse(cartApplicationService.addItem(mapper.toCommand(customerId, request))));
    }

    @PatchMapping("/items/{itemId}")
    public ResponseEntity<CartResponse> updateItem(
            @RequestHeader("X-User-Id") UUID customerId,
            @PathVariable UUID itemId,
            @Valid @RequestBody UpdateCartItemRequest request
    ) {
        return ResponseEntity.ok(
                mapper.toResponse(cartApplicationService.updateItem(mapper.toCommand(customerId, itemId, request)))
        );
    }

    @DeleteMapping("/items/{itemId}")
    public ResponseEntity<CartResponse> removeItem(
            @RequestHeader("X-User-Id") UUID customerId,
            @PathVariable UUID itemId
    ) {
        return ResponseEntity.ok(mapper.toResponse(cartApplicationService.removeItem(customerId, itemId)));
    }

    @DeleteMapping
    public ResponseEntity<CartResponse> clearCart(
            @RequestHeader("X-User-Id") UUID customerId
    ) {
        return ResponseEntity.ok(mapper.toResponse(cartApplicationService.clearCart(customerId)));
    }

    @PostMapping("/checkout")
    public ResponseEntity<OrderCreatedResponse> checkout(
            @RequestHeader("X-User-Id") UUID customerId,
            @Valid @RequestBody CheckoutCartRequest request
    ) {
        return ResponseEntity.status(201)
                .body(orderApiMapper.toResponse(cartApplicationService.checkout(mapper.toCommand(customerId, request))));
    }
}

package com.aifashionstudio.ordering.api.controller;

import com.aifashionstudio.ordering.api.dto.CreateOrderRequest;
import com.aifashionstudio.ordering.api.dto.OrderCreatedResponse;
import com.aifashionstudio.ordering.api.dto.OrderDetailResponse;
import com.aifashionstudio.ordering.api.dto.PagedOrderResponse;
import com.aifashionstudio.ordering.api.mapper.OrderApiMapper;
import com.aifashionstudio.ordering.application.service.OrderApplicationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.UUID;

@RestController
@RequestMapping("/api/orders")
@RequiredArgsConstructor
public class OrderController {

    private final OrderApplicationService orderApplicationService;
    private final OrderApiMapper mapper;

    @PostMapping
    public ResponseEntity<OrderCreatedResponse> createOrder(
            @RequestHeader("X-User-Id") UUID customerId,
            @Valid @RequestBody CreateOrderRequest request
    ) {
        return ResponseEntity.status(201)
                .body(mapper.toResponse(orderApplicationService.createOrder(mapper.toCommand(customerId, request))));
    }

    @GetMapping("/my")
    public ResponseEntity<PagedOrderResponse> getMyOrders(
            @RequestHeader("X-User-Id") UUID customerId,
            @RequestParam(defaultValue = "1") int page,
            @RequestParam(defaultValue = "10") int pageSize
    ) {
        return ResponseEntity.ok(mapper.toResponse(orderApplicationService.getMyOrders(customerId, page, pageSize)));
    }

    @GetMapping("/{orderId}")
    public ResponseEntity<OrderDetailResponse> getOrderDetail(
            @RequestHeader("X-User-Id") UUID requesterId,
            @RequestHeader(value = "X-User-Role", required = false) String userRole,
            @PathVariable UUID orderId
    ) {
        return ResponseEntity.ok(mapper.toResponse(orderApplicationService.getOrderDetail(
                requesterId,
                isStaffOrAdmin(userRole),
                orderId
        )));
    }

    private boolean isStaffOrAdmin(String userRole) {
        return "STAFF".equalsIgnoreCase(userRole) || "ADMIN".equalsIgnoreCase(userRole);
    }
}

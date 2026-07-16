package com.aifashionstudio.ordering.api.controller;

import com.aifashionstudio.ordering.api.dto.OrderStatusUpdatedResponse;
import com.aifashionstudio.ordering.api.dto.UpdateOrderStatusRequest;
import com.aifashionstudio.ordering.api.mapper.OrderApiMapper;
import com.aifashionstudio.ordering.application.command.UpdateOrderStatusCommand;
import com.aifashionstudio.ordering.application.service.OrderApplicationService;
import com.aifashionstudio.shared.exception.ForbiddenException;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PatchMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.UUID;

@RestController
@RequestMapping("/api/staff/orders")
@RequiredArgsConstructor
public class StaffOrderController {

    private final OrderApplicationService orderApplicationService;
    private final OrderApiMapper mapper;

    @PatchMapping("/{orderId}/status")
    public ResponseEntity<OrderStatusUpdatedResponse> updateOrderStatus(
            @RequestHeader("X-User-Id") UUID staffId,
            @RequestHeader(value = "X-User-Role", required = false) String userRole,
            @PathVariable UUID orderId,
            @Valid @RequestBody UpdateOrderStatusRequest request
    ) {
        if (!isStaffOrAdmin(userRole)) {
            throw new ForbiddenException("FORBIDDEN", "Role not allowed");
        }
        return ResponseEntity.ok(mapper.toResponse(orderApplicationService.updateOrderStatus(
                new UpdateOrderStatusCommand(staffId, orderId, request.toStatus(), request.note())
        )));
    }

    private boolean isStaffOrAdmin(String userRole) {
        return "STAFF".equalsIgnoreCase(userRole) || "ADMIN".equalsIgnoreCase(userRole);
    }
}

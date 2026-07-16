package com.aifashionstudio.ordering.application.service.impl;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.model.DesignStatus;
import com.aifashionstudio.design.domain.repository.DesignRepository;
import com.aifashionstudio.ordering.application.command.CreateOrderCommand;
import com.aifashionstudio.ordering.application.command.PaymentSucceededCommand;
import com.aifashionstudio.ordering.application.command.UpdateOrderStatusCommand;
import com.aifashionstudio.ordering.application.dto.OrderCreatedResult;
import com.aifashionstudio.ordering.application.dto.OrderDetailResult;
import com.aifashionstudio.ordering.application.dto.OrderStatusUpdatedResult;
import com.aifashionstudio.ordering.application.dto.PagedOrderResult;
import com.aifashionstudio.ordering.application.mapper.OrderApplicationMapper;
import com.aifashionstudio.ordering.application.service.OrderApplicationService;
import com.aifashionstudio.ordering.domain.model.Order;
import com.aifashionstudio.ordering.domain.model.OrderItem;
import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.domain.model.OrderStatusHistory;
import com.aifashionstudio.ordering.domain.model.PaymentStatus;
import com.aifashionstudio.ordering.domain.repository.OrderRepository;
import com.aifashionstudio.ordering.domain.repository.OrderStatusHistoryRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ForbiddenException;
import com.aifashionstudio.shared.exception.NotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.Map;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class OrderApplicationServiceImpl implements OrderApplicationService {

    private final CatalogRepository catalogRepository;
    private final ProductVariantRepository productVariantRepository;
    private final ProductInventoryRepository productInventoryRepository;
    private final DesignRepository designRepository;
    private final OrderRepository orderRepository;
    private final OrderStatusHistoryRepository orderStatusHistoryRepository;
    private final OrderApplicationMapper mapper;

    @Override
    @Transactional
    public OrderCreatedResult createOrder(CreateOrderCommand command) {
        if (command.items() == null || command.items().isEmpty()) {
            throw new BusinessRuleException("ORDER_ITEMS_REQUIRED", "Order items are required");
        }

        List<OrderItem> items = command.items().stream()
                .map(item -> buildOrderItem(command.customerId(), item))
                .toList();

        Order order = Order.create(
                command.customerId(),
                generateOrderCode(),
                command.receiverName(),
                command.receiverPhone(),
                command.shippingAddress(),
                items
        );

        Order savedOrder = orderRepository.save(order);
        orderStatusHistoryRepository.save(OrderStatusHistory.create(
                savedOrder.getId(),
                null,
                savedOrder.getOrderStatus(),
                command.customerId(),
                "Order created"
        ));

        return mapper.toCreatedResult(savedOrder);
    }

    @Override
    public PagedOrderResult getMyOrders(UUID customerId, int page, int pageSize) {
        validatePage(page, pageSize);
        long totalItems = orderRepository.countByCustomerId(customerId);
        return new PagedOrderResult(
                orderRepository.findByCustomerId(customerId, page, pageSize).stream()
                        .map(mapper::toSummaryResult)
                        .toList(),
                page,
                pageSize,
                totalItems,
                totalPages(totalItems, pageSize)
        );
    }

    @Override
    public OrderDetailResult getOrderDetail(UUID requesterId, boolean staffOrAdmin, UUID orderId) {
        Order order = getOrderOrThrow(orderId);
        if (!staffOrAdmin && !order.getCustomerId().equals(requesterId)) {
            throw new ForbiddenException("ORDER_ACCESS_DENIED", "Order access denied");
        }

        return mapper.toDetailResult(order, orderStatusHistoryRepository.findByOrderId(order.getId()));
    }

    @Override
    @Transactional
    public OrderStatusUpdatedResult updateOrderStatus(UpdateOrderStatusCommand command) {
        Order order = getOrderOrThrow(command.orderId());
        OrderStatus fromStatus;
        try {
            fromStatus = order.updateStatus(command.toStatus());
        } catch (IllegalStateException ex) {
            throw new BusinessRuleException("ORDER_NOT_PAID", ex.getMessage());
        } catch (IllegalArgumentException ex) {
            throw new BusinessRuleException("INVALID_ORDER_STATUS_TRANSITION", ex.getMessage());
        }

        Order savedOrder = orderRepository.save(order);
        orderStatusHistoryRepository.save(OrderStatusHistory.create(
                savedOrder.getId(),
                fromStatus,
                savedOrder.getOrderStatus(),
                command.staffId(),
                command.note()
        ));

        return new OrderStatusUpdatedResult(savedOrder.getId(), fromStatus, savedOrder.getOrderStatus());
    }

    @Override
    @Transactional
    public void handlePaymentSucceeded(PaymentSucceededCommand command) {
        Order order = getOrderOrThrow(command.orderId());
        if (!order.getCustomerId().equals(command.customerId())) {
            throw new BusinessRuleException("PAYMENT_ORDER_MISMATCH", "Payment customer does not match order");
        }
        if (order.getTotalAmount().compareTo(command.amount()) != 0) {
            throw new BusinessRuleException("PAYMENT_AMOUNT_MISMATCH", "Payment amount does not match order total");
        }
        if (order.getPaymentStatus() == PaymentStatus.PAID) {
            return;
        }

        OrderStatus previousStatus = order.getOrderStatus();
        order.markPaid();
        Order savedOrder = orderRepository.save(order);

        for (OrderItem item : savedOrder.getItems()) {
            if (item.getDesignId() != null) {
                Design design = designRepository.findById(item.getDesignId())
                        .orElseThrow(() -> new NotFoundException("DESIGN_NOT_FOUND", "Design not found with id: " + item.getDesignId()));
                try {
                    design.lock();
                } catch (IllegalStateException ex) {
                    throw new BusinessRuleException("DESIGN_MUST_BE_SAVED", ex.getMessage());
                }
                designRepository.save(design);
            }

            ProductInventory inventory = productInventoryRepository.findByProductVariantId(item.getProductVariantId())
                    .orElseThrow(() -> new BusinessRuleException("PRODUCT_OUT_OF_STOCK", "Product inventory not found"));
            try {
                inventory.markSoldFromReserved(item.getQuantity());
            } catch (IllegalStateException ex) {
                throw new BusinessRuleException("PRODUCT_OUT_OF_STOCK", ex.getMessage());
            }
            productInventoryRepository.save(inventory);
        }

        orderStatusHistoryRepository.save(OrderStatusHistory.create(
                savedOrder.getId(),
                previousStatus,
                savedOrder.getOrderStatus(),
                command.customerId(),
                "Payment succeeded: " + command.transactionCode()
        ));
    }

    private OrderItem buildOrderItem(UUID customerId, CreateOrderCommand.CreateOrderItemCommand item) {
        if (item.quantity() <= 0) {
            throw new BusinessRuleException("INVALID_QUANTITY", "Quantity must be greater than zero");
        }

        Catalog product = catalogRepository.findById(item.productId())
                .orElseThrow(() -> new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + item.productId()));
        if (product.getStatus() != CatalogStatus.ACTIVE) {
            throw new BusinessRuleException("PRODUCT_NOT_AVAILABLE", "Product is not available");
        }

        ProductVariant variant = productVariantRepository.findById(item.productVariantId())
                .orElseThrow(() -> new NotFoundException("VARIANT_NOT_FOUND", "Variant not found with id: " + item.productVariantId()));
        if (!variant.getProduct().getId().equals(product.getId()) || variant.getStatus() != ProductVariantStatus.ACTIVE) {
            throw new BusinessRuleException("VARIANT_NOT_AVAILABLE", "Variant is not available");
        }

        UUID designId = null;
        if (item.designId() != null) {
            Design design = designRepository.findById(item.designId())
                    .orElseThrow(() -> new NotFoundException("DESIGN_NOT_FOUND", "Design not found with id: " + item.designId()));
            if (!design.getCustomerId().equals(customerId)) {
                throw new ForbiddenException("DESIGN_ACCESS_DENIED", "Design access denied");
            }
            if (design.getStatus() != DesignStatus.SAVED) {
                throw new BusinessRuleException("DESIGN_MUST_BE_SAVED", "Design must be saved before ordering");
            }
            if (!design.getProductId().equals(product.getId()) || !design.getProductVariantId().equals(variant.getId())) {
                throw new BusinessRuleException("DESIGN_PRODUCT_MISMATCH", "Design does not match requested product variant");
            }
            designId = design.getId();
        }

        ProductInventory inventory = productInventoryRepository.findByProductVariantId(variant.getId())
                .orElseThrow(() -> new BusinessRuleException("PRODUCT_OUT_OF_STOCK", "Product is out of stock"));
        try {
            inventory.reserve(item.quantity());
        } catch (IllegalStateException ex) {
            throw new BusinessRuleException("PRODUCT_OUT_OF_STOCK", ex.getMessage());
        }
        productInventoryRepository.save(inventory);

        BigDecimal unitPrice = product.getBasePrice().add(variant.getPriceAdjustment());
        return OrderItem.create(
                product.getId(),
                variant.getId(),
                designId,
                product.getName(),
                Map.of(
                        "size", variant.getSize(),
                        "color", variant.getColor(),
                        "material", variant.getMaterial()
                ),
                item.quantity(),
                unitPrice
        );
    }

    private String generateOrderCode() {
        return "ORD" + LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyyMMddHHmmssSSS"));
    }

    private Order getOrderOrThrow(UUID orderId) {
        return orderRepository.findById(orderId)
                .orElseThrow(() -> new NotFoundException("ORDER_NOT_FOUND", "Order not found with id: " + orderId));
    }

    private void validatePage(int page, int pageSize) {
        if (page < 1) {
            throw new BusinessRuleException("INVALID_PAGE", "Page must be greater than zero");
        }
        if (pageSize < 1 || pageSize > 100) {
            throw new BusinessRuleException("INVALID_PAGE_SIZE", "Page size must be between 1 and 100");
        }
    }

    private int totalPages(long totalItems, int pageSize) {
        return totalItems == 0 ? 0 : (int) Math.ceil((double) totalItems / pageSize);
    }
}

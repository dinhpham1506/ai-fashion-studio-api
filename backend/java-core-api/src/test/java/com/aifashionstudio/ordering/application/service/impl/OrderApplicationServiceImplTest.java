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
import com.aifashionstudio.ordering.application.mapper.OrderApplicationMapper;
import com.aifashionstudio.ordering.domain.model.Order;
import com.aifashionstudio.ordering.domain.model.OrderItem;
import com.aifashionstudio.ordering.domain.model.OrderStatus;
import com.aifashionstudio.ordering.domain.model.OrderStatusHistory;
import com.aifashionstudio.ordering.domain.model.PaymentStatus;
import com.aifashionstudio.ordering.domain.repository.OrderRepository;
import com.aifashionstudio.ordering.domain.repository.OrderStatusHistoryRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ForbiddenException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class OrderApplicationServiceImplTest {

    private FakeCatalogRepository catalogRepository;
    private FakeProductVariantRepository variantRepository;
    private FakeProductInventoryRepository inventoryRepository;
    private FakeDesignRepository designRepository;
    private FakeOrderRepository orderRepository;
    private FakeOrderStatusHistoryRepository historyRepository;
    private OrderApplicationServiceImpl service;

    @BeforeEach
    void setUp() {
        catalogRepository = new FakeCatalogRepository();
        variantRepository = new FakeProductVariantRepository();
        inventoryRepository = new FakeProductInventoryRepository();
        designRepository = new FakeDesignRepository();
        orderRepository = new FakeOrderRepository();
        historyRepository = new FakeOrderStatusHistoryRepository();
        service = new OrderApplicationServiceImpl(
                catalogRepository,
                variantRepository,
                inventoryRepository,
                designRepository,
                orderRepository,
                historyRepository,
                new OrderApplicationMapper()
        );
    }

    @Test
    void createOrderCreatesPendingPaymentOrderAndReservesInventory() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, new BigDecimal("30000"));
        Design design = design(UUID.randomUUID(), customerId, product.getId(), variant.getId(), DesignStatus.SAVED);
        ProductInventory inventory = inventory(variant, 5);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        designRepository.design = design;
        inventoryRepository.inventory = inventory;

        var result = service.createOrder(command(customerId, product.getId(), variant.getId(), design.getId(), 2));

        assertThat(result.paymentStatus()).isEqualTo(PaymentStatus.PENDING);
        assertThat(result.orderStatus()).isEqualTo(OrderStatus.PENDING_PAYMENT);
        assertThat(result.totalAmount()).isEqualByComparingTo("360000");
        assertThat(inventoryRepository.inventory.getAvailableQuantity()).isEqualTo(3);
        assertThat(inventoryRepository.inventory.getReservedQuantity()).isEqualTo(2);
        assertThat(orderRepository.order.getItems()).hasSize(1);
        assertThat(historyRepository.history.getToStatus()).isEqualTo(OrderStatus.PENDING_PAYMENT);
    }

    @Test
    void createOrderAllowsReadyMadeProductWithoutDesign() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, new BigDecimal("30000"));
        ProductInventory inventory = inventory(variant, 5);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        inventoryRepository.inventory = inventory;

        var result = service.createOrder(command(customerId, product.getId(), variant.getId(), null, 2));

        assertThat(result.paymentStatus()).isEqualTo(PaymentStatus.PENDING);
        assertThat(orderRepository.order.getItems()).hasSize(1);
        assertThat(orderRepository.order.getItems().get(0).getDesignId()).isNull();
        assertThat(inventoryRepository.inventory.getReservedQuantity()).isEqualTo(2);
    }

    @Test
    void handlePaymentSucceededSkipsDesignLockForReadyMadeProduct() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, new BigDecimal("30000"));
        ProductInventory inventory = inventory(variant, 5);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        inventoryRepository.inventory = inventory;

        var created = service.createOrder(command(customerId, product.getId(), variant.getId(), null, 2));

        service.handlePaymentSucceeded(new PaymentSucceededCommand(
                UUID.randomUUID(),
                created.orderId(),
                customerId,
                created.totalAmount(),
                "PAYOS",
                "FT123",
                OffsetDateTime.now(),
                "INV001",
                "https://cdn.example.com/invoice.pdf"
        ));

        assertThat(orderRepository.order.getPaymentStatus()).isEqualTo(PaymentStatus.PAID);
        assertThat(inventoryRepository.inventory.getSoldQuantity()).isEqualTo(2);
    }

    @Test
    void createOrderRejectsUnsavedDesign() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, BigDecimal.ZERO);
        Design design = design(UUID.randomUUID(), customerId, product.getId(), variant.getId(), DesignStatus.DRAFT);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        designRepository.design = design;

        assertThatThrownBy(() -> service.createOrder(command(customerId, product.getId(), variant.getId(), design.getId(), 1)))
                .isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void createOrderRejectsNonOwnerDesign() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, BigDecimal.ZERO);
        Design design = design(UUID.randomUUID(), UUID.randomUUID(), product.getId(), variant.getId(), DesignStatus.SAVED);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        designRepository.design = design;

        assertThatThrownBy(() -> service.createOrder(command(customerId, product.getId(), variant.getId(), design.getId(), 1)))
                .isInstanceOf(ForbiddenException.class);
    }

    @Test
    void createOrderRejectsOutOfStock() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, BigDecimal.ZERO);
        Design design = design(UUID.randomUUID(), customerId, product.getId(), variant.getId(), DesignStatus.SAVED);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        designRepository.design = design;
        inventoryRepository.inventory = inventory(variant, 0);

        assertThatThrownBy(() -> service.createOrder(command(customerId, product.getId(), variant.getId(), design.getId(), 1)))
                .isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void getMyOrdersReturnsOnlyCustomerOrders() {
        UUID customerId = UUID.randomUUID();
        Order first = order(customerId);
        Order second = order(UUID.randomUUID());
        orderRepository.save(first);
        orderRepository.save(second);

        var result = service.getMyOrders(customerId, 1, 10);

        assertThat(result.totalItems()).isEqualTo(1);
        assertThat(result.items()).hasSize(1);
        assertThat(result.items().get(0).id()).isEqualTo(first.getId());
    }

    @Test
    void getOrderDetailRejectsOtherCustomer() {
        Order order = order(UUID.randomUUID());
        orderRepository.save(order);

        assertThatThrownBy(() -> service.getOrderDetail(UUID.randomUUID(), false, order.getId()))
                .isInstanceOf(ForbiddenException.class);
    }

    @Test
    void updateOrderStatusMovesPaidOrderToProduction() {
        Order order = order(UUID.randomUUID());
        order.markPaid();
        orderRepository.save(order);

        var result = service.updateOrderStatus(new UpdateOrderStatusCommand(
                UUID.randomUUID(),
                order.getId(),
                OrderStatus.IN_PRODUCTION,
                "Start printing"
        ));

        assertThat(result.fromStatus()).isEqualTo(OrderStatus.PAID);
        assertThat(result.toStatus()).isEqualTo(OrderStatus.IN_PRODUCTION);
        assertThat(historyRepository.histories).hasSize(1);
    }

    @Test
    void updateOrderStatusRejectsUnpaidOrder() {
        Order order = order(UUID.randomUUID());
        orderRepository.save(order);

        assertThatThrownBy(() -> service.updateOrderStatus(new UpdateOrderStatusCommand(
                UUID.randomUUID(),
                order.getId(),
                OrderStatus.IN_PRODUCTION,
                null
        ))).isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void handlePaymentSucceededMarksOrderPaidLocksDesignAndMarksInventorySold() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, new BigDecimal("30000"));
        Design design = design(UUID.randomUUID(), customerId, product.getId(), variant.getId(), DesignStatus.SAVED);
        ProductInventory inventory = inventory(variant, 5);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        designRepository.design = design;
        inventoryRepository.inventory = inventory;

        var created = service.createOrder(command(customerId, product.getId(), variant.getId(), design.getId(), 2));

        service.handlePaymentSucceeded(new PaymentSucceededCommand(
                UUID.randomUUID(),
                created.orderId(),
                customerId,
                created.totalAmount(),
                "PAYOS",
                "FT123",
                OffsetDateTime.now(),
                "INV001",
                "https://cdn.example.com/invoice.pdf"
        ));

        assertThat(orderRepository.order.getPaymentStatus()).isEqualTo(PaymentStatus.PAID);
        assertThat(orderRepository.order.getOrderStatus()).isEqualTo(OrderStatus.PAID);
        assertThat(designRepository.design.getStatus()).isEqualTo(DesignStatus.LOCKED);
        assertThat(inventoryRepository.inventory.getReservedQuantity()).isZero();
        assertThat(inventoryRepository.inventory.getSoldQuantity()).isEqualTo(2);
    }

    @Test
    void handlePaymentSucceededIgnoresDuplicatePaidOrder() {
        UUID customerId = UUID.randomUUID();
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE, new BigDecimal("150000"));
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE, new BigDecimal("30000"));
        Design design = design(UUID.randomUUID(), customerId, product.getId(), variant.getId(), DesignStatus.SAVED);
        ProductInventory inventory = inventory(variant, 5);
        catalogRepository.catalog = product;
        variantRepository.variant = variant;
        designRepository.design = design;
        inventoryRepository.inventory = inventory;

        var created = service.createOrder(command(customerId, product.getId(), variant.getId(), design.getId(), 2));
        var command = new PaymentSucceededCommand(
                UUID.randomUUID(),
                created.orderId(),
                customerId,
                created.totalAmount(),
                "PAYOS",
                "FT123",
                OffsetDateTime.now(),
                "INV001",
                "https://cdn.example.com/invoice.pdf"
        );

        service.handlePaymentSucceeded(command);
        service.handlePaymentSucceeded(command);

        assertThat(orderRepository.order.getPaymentStatus()).isEqualTo(PaymentStatus.PAID);
        assertThat(inventoryRepository.inventory.getReservedQuantity()).isZero();
        assertThat(inventoryRepository.inventory.getSoldQuantity()).isEqualTo(2);
        assertThat(historyRepository.histories)
                .extracting(OrderStatusHistory::getToStatus)
                .containsExactly(OrderStatus.PENDING_PAYMENT, OrderStatus.PAID);
    }

    private CreateOrderCommand command(UUID customerId, UUID productId, UUID variantId, UUID designId, int quantity) {
        return new CreateOrderCommand(
                customerId,
                List.of(new CreateOrderCommand.CreateOrderItemCommand(productId, variantId, designId, quantity)),
                "Nguyen Van A",
                "0909000000",
                "123 Nguyen Trai"
        );
    }

    private Catalog catalog(UUID id, CatalogStatus status, BigDecimal basePrice) {
        return Catalog.reconstitute(
                id,
                "Basic Tee",
                "Description",
                basePrice,
                status,
                UUID.randomUUID(),
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private ProductVariant variant(UUID id, Catalog product, ProductVariantStatus status, BigDecimal priceAdjustment) {
        return ProductVariant.reconstitute(
                id,
                product,
                "SKU-001",
                "M",
                "White",
                "Cotton",
                priceAdjustment,
                status,
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private ProductInventory inventory(ProductVariant variant, int availableQuantity) {
        return ProductInventory.reconstitute(
                UUID.randomUUID(),
                variant,
                availableQuantity,
                0,
                0,
                OffsetDateTime.now()
        );
    }

    private Design design(UUID id, UUID customerId, UUID productId, UUID variantId, DesignStatus status) {
        return Design.reconstitute(
                id,
                customerId,
                productId,
                variantId,
                "Saved design",
                java.util.Map.of(),
                "https://cdn.example.com/preview.png",
                "https://cdn.example.com/print.pdf",
                status,
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private Order order(UUID customerId) {
        return Order.create(
                customerId,
                "ORD-" + UUID.randomUUID(),
                "Nguyen Van A",
                "0909000000",
                "123 Nguyen Trai",
                List.of(OrderItem.create(
                        UUID.randomUUID(),
                        UUID.randomUUID(),
                        UUID.randomUUID(),
                        "Basic Tee",
                        java.util.Map.of("size", "M", "color", "White", "material", "Cotton"),
                        1,
                        new BigDecimal("150000")
                ))
        );
    }

    private static class FakeCatalogRepository implements CatalogRepository {
        private Catalog catalog;

        @Override
        public Catalog save(Catalog catalog) {
            this.catalog = catalog;
            return catalog;
        }

        @Override
        public Optional<Catalog> findById(UUID id) {
            return Optional.ofNullable(catalog).filter(item -> item.getId().equals(id));
        }

        @Override
        public List<Catalog> findAll() {
            return catalog == null ? List.of() : List.of(catalog);
        }

        @Override
        public List<Catalog> findByStatus(CatalogStatus status) {
            return catalog != null && catalog.getStatus() == status ? List.of(catalog) : List.of();
        }

        @Override
        public List<Catalog> findByNameContainingIgnoreCase(String name) {
            return List.of();
        }

        @Override
        public List<Catalog> findByStatusAndNameContainingIgnoreCase(CatalogStatus status, String name) {
            return List.of();
        }

        @Override
        public boolean existsByNameIgnoreCase(String name) {
            return false;
        }

        @Override
        public boolean existsByNameIgnoreCaseAndIdNot(String name, UUID id) {
            return false;
        }
    }

    private static class FakeProductVariantRepository implements ProductVariantRepository {
        private ProductVariant variant;

        @Override
        public ProductVariant save(ProductVariant productVariant) {
            this.variant = productVariant;
            return productVariant;
        }

        @Override
        public Optional<ProductVariant> findById(UUID id) {
            return Optional.ofNullable(variant).filter(item -> item.getId().equals(id));
        }

        @Override
        public List<ProductVariant> findByProductId(UUID productId) {
            return variant != null && variant.getProduct().getId().equals(productId) ? List.of(variant) : List.of();
        }

        @Override
        public List<ProductVariant> findByProductIdAndStatus(UUID productId, ProductVariantStatus status) {
            return variant != null && variant.getProduct().getId().equals(productId) && variant.getStatus() == status
                    ? List.of(variant) : List.of();
        }

        @Override
        public Optional<ProductVariant> findBySku(String sku) {
            return Optional.empty();
        }

        @Override
        public boolean existsBySku(String sku) {
            return false;
        }

        @Override
        public boolean existsBySkuAndIdNot(String sku, UUID id) {
            return false;
        }
    }

    private static class FakeProductInventoryRepository implements ProductInventoryRepository {
        private ProductInventory inventory;

        @Override
        public ProductInventory save(ProductInventory productInventory) {
            this.inventory = productInventory;
            return productInventory;
        }

        @Override
        public Optional<ProductInventory> findByProductVariantId(UUID productVariantId) {
            return Optional.ofNullable(inventory)
                    .filter(item -> item.getProductVariant().getId().equals(productVariantId));
        }

        @Override
        public boolean existsByProductVariantId(UUID productVariantId) {
            return findByProductVariantId(productVariantId).isPresent();
        }
    }

    private static class FakeDesignRepository implements DesignRepository {
        private Design design;

        @Override
        public Design save(Design design) {
            this.design = design;
            return design;
        }

        @Override
        public Optional<Design> findById(UUID id) {
            return Optional.ofNullable(design).filter(item -> item.getId().equals(id));
        }

        @Override
        public List<Design> findByCustomerId(UUID customerId, int page, int pageSize) {
            return design != null && design.getCustomerId().equals(customerId) ? List.of(design) : List.of();
        }

        @Override
        public long countByCustomerId(UUID customerId) {
            return findByCustomerId(customerId, 1, 10).size();
        }
    }

    private static class FakeOrderRepository implements OrderRepository {
        private Order order;
        private final List<Order> orders = new ArrayList<>();

        @Override
        public Order save(Order order) {
            if (order.getId() == null) {
                order.setId(UUID.randomUUID());
            }
            order.getItems().forEach(item -> item.assignOrderId(order.getId()));
            orders.removeIf(item -> item.getId().equals(order.getId()));
            orders.add(order);
            this.order = order;
            return order;
        }

        @Override
        public Optional<Order> findById(UUID id) {
            return orders.stream().filter(item -> item.getId().equals(id)).findFirst();
        }

        @Override
        public List<Order> findByCustomerId(UUID customerId, int page, int pageSize) {
            return orders.stream()
                    .filter(item -> item.getCustomerId().equals(customerId))
                    .skip((long) (page - 1) * pageSize)
                    .limit(pageSize)
                    .toList();
        }

        @Override
        public long countByCustomerId(UUID customerId) {
            return orders.stream().filter(item -> item.getCustomerId().equals(customerId)).count();
        }

        @Override
        public List<Order> findByOrderStatus(OrderStatus status, int page, int pageSize) {
            return orders.stream()
                    .filter(item -> item.getOrderStatus() == status)
                    .skip((long) (page - 1) * pageSize)
                    .limit(pageSize)
                    .toList();
        }

        @Override
        public long countByOrderStatus(OrderStatus status) {
            return orders.stream().filter(item -> item.getOrderStatus() == status).count();
        }
    }

    private static class FakeOrderStatusHistoryRepository implements OrderStatusHistoryRepository {
        private OrderStatusHistory history;
        private final List<OrderStatusHistory> histories = new ArrayList<>();

        @Override
        public OrderStatusHistory save(OrderStatusHistory history) {
            if (history.getId() == null) {
                history.setId(UUID.randomUUID());
            }
            this.history = history;
            this.histories.add(history);
            return history;
        }

        @Override
        public List<OrderStatusHistory> findByOrderId(UUID orderId) {
            return histories.stream()
                    .filter(item -> item.getOrderId().equals(orderId))
                    .toList();
        }
    }
}

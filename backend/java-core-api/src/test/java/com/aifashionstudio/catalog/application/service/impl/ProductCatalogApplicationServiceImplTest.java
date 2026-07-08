package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.CreateProductVariantCommand;
import com.aifashionstudio.catalog.application.command.UpdateInventoryCommand;
import com.aifashionstudio.catalog.application.mapper.ProductCatalogApplicationMapper;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ConflictException;
import com.aifashionstudio.shared.exception.NotFoundException;
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

class ProductCatalogApplicationServiceImplTest {

    private FakeCatalogRepository catalogRepository;
    private FakeProductVariantRepository variantRepository;
    private FakeProductInventoryRepository inventoryRepository;
    private ProductVariantApplicationServiceImpl variantService;
    private ProductInventoryApplicationServiceImpl inventoryService;

    @BeforeEach
    void setUp() {
        catalogRepository = new FakeCatalogRepository();
        variantRepository = new FakeProductVariantRepository();
        inventoryRepository = new FakeProductInventoryRepository();
        ProductCatalogApplicationMapper mapper = new ProductCatalogApplicationMapper();

        variantService = new ProductVariantApplicationServiceImpl(
                catalogRepository,
                variantRepository,
                inventoryRepository,
                mapper
        );
        inventoryService = new ProductInventoryApplicationServiceImpl(
                variantRepository,
                inventoryRepository,
                mapper
        );
    }

    @Test
    void createVariantRejectsDuplicateSku() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        catalogRepository.catalog = product;
        variantRepository.duplicateSku = true;

        assertThatThrownBy(() -> variantService.createVariant(product.getId(), new CreateProductVariantCommand(
                "SKU-001",
                "M",
                "White",
                "Cotton",
                BigDecimal.ZERO
        ))).isInstanceOf(ConflictException.class);

        assertThat(variantRepository.saveCalls).isZero();
    }

    @Test
    void createVariantCreatesDefaultInventory() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        catalogRepository.catalog = product;

        var result = variantService.createVariant(product.getId(), new CreateProductVariantCommand(
                " SKU-001 ",
                " M ",
                " White ",
                " Cotton ",
                new BigDecimal("10000")
        ));

        assertThat(result.sku()).isEqualTo("SKU-001");
        assertThat(result.size()).isEqualTo("M");
        assertThat(result.inventory().availableQuantity()).isZero();
        assertThat(inventoryRepository.saveCalls).isEqualTo(1);
    }

    @Test
    void getPublicVariantByIdHidesInactiveVariant() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.INACTIVE);
        variantRepository.variant = variant;

        assertThatThrownBy(() -> variantService.getPublicVariantById(variant.getId()))
                .isInstanceOf(NotFoundException.class);
    }

    @Test
    void getPublicVariantsByProductIdHidesInactiveProduct() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.INACTIVE);
        catalogRepository.catalog = product;

        assertThatThrownBy(() -> variantService.getPublicVariantsByProductId(product.getId()))
                .isInstanceOf(NotFoundException.class);
    }

    @Test
    void updateInventoryRejectsNegativeAvailableQuantity() {
        assertThatThrownBy(() -> inventoryService.updateInventory(
                UUID.randomUUID(),
                new UpdateInventoryCommand(-1)
        )).isInstanceOf(BusinessRuleException.class);
    }

    private Catalog catalog(UUID id, CatalogStatus status) {
        return Catalog.reconstitute(
                id,
                "Tee",
                "Description",
                new BigDecimal("100000"),
                status,
                UUID.randomUUID(),
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private ProductVariant variant(UUID id, Catalog product, ProductVariantStatus status) {
        ProductVariant variant = new ProductVariant();
        variant.setId(id);
        variant.setProduct(product);
        variant.setSku("SKU-001");
        variant.setSize("M");
        variant.setColor("White");
        variant.setMaterial("Cotton");
        variant.setPriceAdjustment(BigDecimal.ZERO);
        variant.setStatus(status);
        return variant;
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
            return Optional.ofNullable(catalog)
                    .filter(item -> item.getId().equals(id));
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
        private boolean duplicateSku;
        private int saveCalls;
        private final List<ProductVariant> variants = new ArrayList<>();

        @Override
        public ProductVariant save(ProductVariant productVariant) {
            if (productVariant.getId() == null) {
                productVariant.setId(UUID.randomUUID());
            }
            this.variant = productVariant;
            this.variants.add(productVariant);
            this.saveCalls++;
            return productVariant;
        }

        @Override
        public Optional<ProductVariant> findById(UUID id) {
            return Optional.ofNullable(variant)
                    .filter(item -> item.getId().equals(id));
        }

        @Override
        public List<ProductVariant> findByProductId(UUID productId) {
            return variants.stream()
                    .filter(item -> item.getProduct().getId().equals(productId))
                    .toList();
        }

        @Override
        public List<ProductVariant> findByProductIdAndStatus(UUID productId, ProductVariantStatus status) {
            return variants.stream()
                    .filter(item -> item.getProduct().getId().equals(productId))
                    .filter(item -> item.getStatus() == status)
                    .toList();
        }

        @Override
        public Optional<ProductVariant> findBySku(String sku) {
            return Optional.empty();
        }

        @Override
        public boolean existsBySku(String sku) {
            return duplicateSku;
        }

        @Override
        public boolean existsBySkuAndIdNot(String sku, UUID id) {
            return duplicateSku;
        }
    }

    private static class FakeProductInventoryRepository implements ProductInventoryRepository {
        private ProductInventory inventory;
        private int saveCalls;

        @Override
        public ProductInventory save(ProductInventory productInventory) {
            if (productInventory.getId() == null) {
                productInventory.setId(UUID.randomUUID());
            }
            this.inventory = productInventory;
            this.saveCalls++;
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
}

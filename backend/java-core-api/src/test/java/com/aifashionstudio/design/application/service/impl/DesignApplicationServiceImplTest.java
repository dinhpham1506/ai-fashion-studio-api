package com.aifashionstudio.design.application.service.impl;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.design.application.command.CreateDraftDesignCommand;
import com.aifashionstudio.design.application.command.SaveDesignCommand;
import com.aifashionstudio.design.application.mapper.DesignApplicationMapper;
import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.model.DesignLayer;
import com.aifashionstudio.design.domain.model.DesignLayerType;
import com.aifashionstudio.design.domain.model.DesignStatus;
import com.aifashionstudio.design.domain.repository.DesignLayerRepository;
import com.aifashionstudio.design.domain.repository.DesignRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ConflictException;
import com.aifashionstudio.shared.exception.ForbiddenException;
import com.aifashionstudio.shared.exception.NotFoundException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class DesignApplicationServiceImplTest {

    private FakeCatalogRepository catalogRepository;
    private FakeProductVariantRepository productVariantRepository;
    private FakeDesignRepository designRepository;
    private FakeDesignLayerRepository designLayerRepository;
    private DesignApplicationServiceImpl service;

    @BeforeEach
    void setUp() {
        catalogRepository = new FakeCatalogRepository();
        productVariantRepository = new FakeProductVariantRepository();
        designRepository = new FakeDesignRepository();
        designLayerRepository = new FakeDesignLayerRepository();
        service = new DesignApplicationServiceImpl(
                catalogRepository,
                productVariantRepository,
                designRepository,
                designLayerRepository,
                new DesignApplicationMapper()
        );
    }

    @Test
    void createDraftCreatesDraftDesignForActiveProductAndVariant() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.ACTIVE);
        catalogRepository.catalog = product;
        productVariantRepository.variant = variant;

        var result = service.createDraft(new CreateDraftDesignCommand(
                UUID.randomUUID(),
                product.getId(),
                variant.getId(),
                " My First Shirt "
        ));

        assertThat(result.designId()).isNotNull();
        assertThat(result.status()).isEqualTo(DesignStatus.DRAFT);
        assertThat(designRepository.savedDesign.getName()).isEqualTo("My First Shirt");
    }

    @Test
    void createDraftRejectsMissingProduct() {
        assertThatThrownBy(() -> service.createDraft(new CreateDraftDesignCommand(
                UUID.randomUUID(),
                UUID.randomUUID(),
                UUID.randomUUID(),
                "Design"
        ))).isInstanceOf(NotFoundException.class);
    }

    @Test
    void createDraftRejectsInactiveProduct() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.INACTIVE);
        catalogRepository.catalog = product;

        assertThatThrownBy(() -> service.createDraft(new CreateDraftDesignCommand(
                UUID.randomUUID(),
                product.getId(),
                UUID.randomUUID(),
                "Design"
        ))).isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void createDraftRejectsInactiveVariant() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        ProductVariant variant = variant(UUID.randomUUID(), product, ProductVariantStatus.INACTIVE);
        catalogRepository.catalog = product;
        productVariantRepository.variant = variant;

        assertThatThrownBy(() -> service.createDraft(new CreateDraftDesignCommand(
                UUID.randomUUID(),
                product.getId(),
                variant.getId(),
                "Design"
        ))).isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void createDraftRejectsVariantFromAnotherProduct() {
        Catalog product = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        Catalog anotherProduct = catalog(UUID.randomUUID(), CatalogStatus.ACTIVE);
        ProductVariant variant = variant(UUID.randomUUID(), anotherProduct, ProductVariantStatus.ACTIVE);
        catalogRepository.catalog = product;
        productVariantRepository.variant = variant;

        assertThatThrownBy(() -> service.createDraft(new CreateDraftDesignCommand(
                UUID.randomUUID(),
                product.getId(),
                variant.getId(),
                "Design"
        ))).isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void saveDesignSavesCanvasAndReplacesLayers() {
        UUID customerId = UUID.randomUUID();
        Design design = draftDesign(UUID.randomUUID(), customerId);
        designRepository.savedDesign = design;
        designLayerRepository.layers.add(DesignLayer.create(
                design.getId(),
                DesignLayerType.TEXT,
                "Old",
                BigDecimal.ZERO,
                BigDecimal.ZERO,
                BigDecimal.ONE,
                BigDecimal.ONE,
                BigDecimal.ZERO,
                null,
                1
        ));

        var result = service.saveDesign(new SaveDesignCommand(
                customerId,
                design.getId(),
                " Saved Shirt ",
                Map.of("version", "5.3.0"),
                " https://cdn.example.com/preview.png ",
                " https://cdn.example.com/print.pdf ",
                List.of(new SaveDesignCommand.SaveDesignLayerCommand(
                        DesignLayerType.TEXT,
                        "HELLO",
                        new BigDecimal("120"),
                        new BigDecimal("180"),
                        new BigDecimal("200"),
                        new BigDecimal("60"),
                        BigDecimal.ZERO,
                        "#000000",
                        1
                ))
        ));

        assertThat(result.status()).isEqualTo(DesignStatus.SAVED);
        assertThat(designRepository.savedDesign.getName()).isEqualTo("Saved Shirt");
        assertThat(designLayerRepository.deleteCalls).isEqualTo(1);
        assertThat(designLayerRepository.layers).hasSize(1);
        assertThat(designLayerRepository.layers.get(0).getContent()).isEqualTo("HELLO");
    }

    @Test
    void saveDesignRejectsNonOwner() {
        Design design = draftDesign(UUID.randomUUID(), UUID.randomUUID());
        designRepository.savedDesign = design;

        assertThatThrownBy(() -> service.saveDesign(new SaveDesignCommand(
                UUID.randomUUID(),
                design.getId(),
                "Design",
                Map.of(),
                null,
                null,
                List.of()
        ))).isInstanceOf(ForbiddenException.class);
    }

    @Test
    void saveDesignRejectsLockedDesign() {
        UUID customerId = UUID.randomUUID();
        Design design = lockedDesign(UUID.randomUUID(), customerId);
        designRepository.savedDesign = design;

        assertThatThrownBy(() -> service.saveDesign(new SaveDesignCommand(
                customerId,
                design.getId(),
                "Design",
                Map.of(),
                null,
                null,
                List.of()
        ))).isInstanceOf(ConflictException.class);
    }

    @Test
    void saveDesignRejectsTooManyLayers() {
        UUID customerId = UUID.randomUUID();
        Design design = draftDesign(UUID.randomUUID(), customerId);
        designRepository.savedDesign = design;

        List<SaveDesignCommand.SaveDesignLayerCommand> layers = new ArrayList<>();
        for (int index = 0; index < 51; index++) {
            layers.add(new SaveDesignCommand.SaveDesignLayerCommand(
                    DesignLayerType.TEXT,
                    "Layer",
                    BigDecimal.ZERO,
                    BigDecimal.ZERO,
                    BigDecimal.ONE,
                    BigDecimal.ONE,
                    BigDecimal.ZERO,
                    null,
                    index
            ));
        }

        assertThatThrownBy(() -> service.saveDesign(new SaveDesignCommand(
                customerId,
                design.getId(),
                "Design",
                Map.of(),
                null,
                null,
                layers
        ))).isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void getMyDesignsReturnsOnlyCustomerDesignsWithPaging() {
        UUID customerId = UUID.randomUUID();
        designRepository.designs.add(draftDesign(UUID.randomUUID(), customerId));
        designRepository.designs.add(draftDesign(UUID.randomUUID(), customerId));
        designRepository.designs.add(draftDesign(UUID.randomUUID(), UUID.randomUUID()));

        var result = service.getMyDesigns(customerId, 1, 1);

        assertThat(result.items()).hasSize(1);
        assertThat(result.totalItems()).isEqualTo(2);
        assertThat(result.totalPages()).isEqualTo(2);
    }

    @Test
    void getMyDesignsRejectsInvalidPage() {
        assertThatThrownBy(() -> service.getMyDesigns(UUID.randomUUID(), 0, 10))
                .isInstanceOf(BusinessRuleException.class);
    }

    @Test
    void getDesignDetailReturnsDesignAndLayersForOwner() {
        UUID customerId = UUID.randomUUID();
        Design design = draftDesign(UUID.randomUUID(), customerId);
        designRepository.savedDesign = design;
        designLayerRepository.layers.add(DesignLayer.create(
                design.getId(),
                DesignLayerType.TEXT,
                "HELLO",
                BigDecimal.ZERO,
                BigDecimal.ZERO,
                BigDecimal.ONE,
                BigDecimal.ONE,
                BigDecimal.ZERO,
                "#000000",
                1
        ));

        var result = service.getDesignDetail(customerId, design.getId());

        assertThat(result.id()).isEqualTo(design.getId());
        assertThat(result.layers()).hasSize(1);
        assertThat(result.layers().get(0).content()).isEqualTo("HELLO");
    }

    @Test
    void getDesignDetailRejectsNonOwner() {
        Design design = draftDesign(UUID.randomUUID(), UUID.randomUUID());
        designRepository.savedDesign = design;

        assertThatThrownBy(() -> service.getDesignDetail(UUID.randomUUID(), design.getId()))
                .isInstanceOf(ForbiddenException.class);
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
        return ProductVariant.reconstitute(
                id,
                product,
                "SKU-001",
                "M",
                "White",
                "Cotton",
                BigDecimal.ZERO,
                status,
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private Design draftDesign(UUID id, UUID customerId) {
        return Design.reconstitute(
                id,
                customerId,
                UUID.randomUUID(),
                UUID.randomUUID(),
                "Draft",
                Map.of(),
                null,
                null,
                DesignStatus.DRAFT,
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private Design lockedDesign(UUID id, UUID customerId) {
        return Design.reconstitute(
                id,
                customerId,
                UUID.randomUUID(),
                UUID.randomUUID(),
                "Locked",
                Map.of(),
                null,
                null,
                DesignStatus.LOCKED,
                OffsetDateTime.now(),
                OffsetDateTime.now()
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

        @Override
        public ProductVariant save(ProductVariant productVariant) {
            this.variant = productVariant;
            return productVariant;
        }

        @Override
        public Optional<ProductVariant> findById(UUID id) {
            return Optional.ofNullable(variant)
                    .filter(item -> item.getId().equals(id));
        }

        @Override
        public List<ProductVariant> findByProductId(UUID productId) {
            return variant != null && variant.getProduct().getId().equals(productId) ? List.of(variant) : List.of();
        }

        @Override
        public List<ProductVariant> findByProductIdAndStatus(UUID productId, ProductVariantStatus status) {
            return variant != null
                    && variant.getProduct().getId().equals(productId)
                    && variant.getStatus() == status ? List.of(variant) : List.of();
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

    private static class FakeDesignRepository implements DesignRepository {
        private Design savedDesign;
        private final List<Design> designs = new ArrayList<>();

        @Override
        public Design save(Design design) {
            if (design.getId() == null) {
                design.setId(UUID.randomUUID());
            }
            savedDesign = design;
            designs.removeIf(item -> item.getId().equals(design.getId()));
            designs.add(design);
            return design;
        }

        @Override
        public Optional<Design> findById(UUID id) {
            return Optional.ofNullable(savedDesign)
                    .filter(item -> item.getId().equals(id));
        }

        @Override
        public List<Design> findByCustomerId(UUID customerId, int page, int pageSize) {
            return designs.stream()
                    .filter(item -> item.getCustomerId().equals(customerId))
                    .skip((long) (page - 1) * pageSize)
                    .limit(pageSize)
                    .toList();
        }

        @Override
        public long countByCustomerId(UUID customerId) {
            return designs.stream()
                    .filter(item -> item.getCustomerId().equals(customerId))
                    .count();
        }
    }

    private static class FakeDesignLayerRepository implements DesignLayerRepository {
        private final List<DesignLayer> layers = new ArrayList<>();
        private int deleteCalls;

        @Override
        public List<DesignLayer> saveAll(List<DesignLayer> layers) {
            layers.forEach(layer -> {
                if (layer.getId() == null) {
                    layer.setId(UUID.randomUUID());
                }
            });
            this.layers.addAll(layers);
            return layers;
        }

        @Override
        public void deleteByDesignId(UUID designId) {
            deleteCalls++;
            layers.removeIf(layer -> layer.getDesignId().equals(designId));
        }

        @Override
        public List<DesignLayer> findByDesignIdOrderByZIndexAsc(UUID designId) {
            return layers.stream()
                    .filter(layer -> layer.getDesignId().equals(designId))
                    .sorted((left, right) -> Integer.compare(left.getZIndex(), right.getZIndex()))
                    .toList();
        }
    }
}

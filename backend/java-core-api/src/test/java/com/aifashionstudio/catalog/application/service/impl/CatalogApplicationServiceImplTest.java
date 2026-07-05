package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.ChangeCatalogStatusCommand;
import com.aifashionstudio.catalog.application.command.UpdateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.application.mapper.CatalogApplicationMapper;
import com.aifashionstudio.catalog.domain.exception.CatalogNameAlreadyExistsException;
import com.aifashionstudio.catalog.domain.exception.InvalidCatalogStatusTransitionException;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.service.CatalogDomainService;
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

class CatalogApplicationServiceImplTest {

    private FakeCatalogRepository catalogRepository;
    private CatalogApplicationServiceImpl service;

    @BeforeEach
    void setUp() {
        catalogRepository = new FakeCatalogRepository();
        service = new CatalogApplicationServiceImpl(
                catalogRepository,
                new NoOpCatalogDomainService(),
                new CatalogApplicationMapper()
        );
    }

    @Test
    void updateCatalogUpdatesDetailsAndSaves() {
        UUID id = UUID.randomUUID();
        Catalog catalog = catalog(id, "Old Tee", "Old description", "100000", CatalogStatus.DRAFT);
        catalogRepository.catalog = catalog;

        CatalogResult result = service.updateCatalog(new UpdateCatalogCommand(
                id,
                " New Tee ",
                "New description",
                new BigDecimal("150000")
        ));

        assertThat(result.name()).isEqualTo("New Tee");
        assertThat(result.description()).isEqualTo("New description");
        assertThat(result.basePrice()).isEqualByComparingTo("150000");
        assertThat(catalogRepository.savedCatalog).isSameAs(catalog);
        assertThat(catalogRepository.saveCalls).isEqualTo(1);
    }

    @Test
    void updateCatalogRejectsDuplicateNameFromAnotherCatalog() {
        UUID id = UUID.randomUUID();
        catalogRepository.catalog = catalog(id, "Old Tee", "Old description", "100000", CatalogStatus.DRAFT);
        catalogRepository.duplicateName = true;

        assertThatThrownBy(() -> service.updateCatalog(new UpdateCatalogCommand(
                id,
                "New Tee",
                "New description",
                new BigDecimal("150000")
        ))).isInstanceOf(CatalogNameAlreadyExistsException.class);

        assertThat(catalogRepository.saveCalls).isZero();
    }

    @Test
    void changeCatalogStatusUpdatesStatusAndSaves() {
        UUID id = UUID.randomUUID();
        Catalog catalog = catalog(id, "Tee", "Description", "100000", CatalogStatus.DRAFT);
        catalogRepository.catalog = catalog;

        CatalogResult result = service.changeCatalogStatus(
                new ChangeCatalogStatusCommand(id, CatalogStatus.ACTIVE)
        );

        assertThat(result.status()).isEqualTo(CatalogStatus.ACTIVE);
        assertThat(catalogRepository.savedCatalog).isSameAs(catalog);
        assertThat(catalogRepository.saveCalls).isEqualTo(1);
    }

    @Test
    void changeCatalogStatusRejectsDraftToArchived() {
        UUID id = UUID.randomUUID();
        catalogRepository.catalog = catalog(id, "Tee", "Description", "100000", CatalogStatus.DRAFT);

        assertThatThrownBy(() -> service.changeCatalogStatus(
                new ChangeCatalogStatusCommand(id, CatalogStatus.ARCHIVED)
        )).isInstanceOf(InvalidCatalogStatusTransitionException.class);

        assertThat(catalogRepository.saveCalls).isZero();
    }

    @Test
    void getPublicProductsReturnsActiveProductsWithNameFilter() {
        Catalog activeCatalog = catalog(UUID.randomUUID(), "White Tee", "Description", "100000", CatalogStatus.ACTIVE);
        catalogRepository.statusAndNameResults = List.of(activeCatalog);

        List<CatalogResult> result = service.getPublicProducts(" tee ");

        assertThat(result).hasSize(1);
        assertThat(result.get(0).name()).isEqualTo("White Tee");
        assertThat(catalogRepository.lastStatusFilter).isEqualTo(CatalogStatus.ACTIVE);
        assertThat(catalogRepository.lastNameFilter).isEqualTo("tee");
    }

    @Test
    void getPublicProductByIdHidesInactiveProducts() {
        UUID id = UUID.randomUUID();
        catalogRepository.catalog = catalog(id, "Tee", "Description", "100000", CatalogStatus.INACTIVE);

        assertThatThrownBy(() -> service.getPublicProductById(id))
                .isInstanceOf(NotFoundException.class);
    }

    private Catalog catalog(UUID id, String name, String description, String basePrice, CatalogStatus status) {
        return Catalog.reconstitute(
                id,
                name,
                description,
                new BigDecimal(basePrice),
                status,
                UUID.randomUUID(),
                OffsetDateTime.now(),
                OffsetDateTime.now()
        );
    }

    private static class FakeCatalogRepository implements CatalogRepository {
        private Catalog catalog;
        private Catalog savedCatalog;
        private int saveCalls;
        private boolean duplicateName;
        private CatalogStatus lastStatusFilter;
        private String lastNameFilter;
        private List<Catalog> statusAndNameResults = new ArrayList<>();

        @Override
        public Catalog save(Catalog catalog) {
            this.savedCatalog = catalog;
            this.saveCalls++;
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
            lastStatusFilter = status;
            return catalog != null && catalog.getStatus() == status ? List.of(catalog) : List.of();
        }

        @Override
        public List<Catalog> findByNameContainingIgnoreCase(String name) {
            lastNameFilter = name;
            return catalog != null && catalog.getName().toLowerCase().contains(name.toLowerCase())
                    ? List.of(catalog)
                    : List.of();
        }

        @Override
        public List<Catalog> findByStatusAndNameContainingIgnoreCase(CatalogStatus status, String name) {
            lastStatusFilter = status;
            lastNameFilter = name;
            return statusAndNameResults;
        }

        @Override
        public boolean existsByNameIgnoreCase(String name) {
            return duplicateName;
        }

        @Override
        public boolean existsByNameIgnoreCaseAndIdNot(String name, UUID id) {
            return duplicateName;
        }
    }

    private static class NoOpCatalogDomainService implements CatalogDomainService {
        @Override
        public void validateCatalogCanBeCreated(String name, BigDecimal basePrice) {
        }

        @Override
        public void validateCatalogCanBeActivated(Catalog catalog) {
        }

        @Override
        public BigDecimal calculateVariantFinalPrice(BigDecimal basePrice, ProductVariant productVariant) {
            return basePrice.add(productVariant.getPriceAdjustment());
        }
    }
}

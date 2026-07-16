package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.ChangeCatalogStatusCommand;
import com.aifashionstudio.catalog.application.command.CreateCatalogCommand;
import com.aifashionstudio.catalog.application.command.UpdateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.application.mapper.CatalogApplicationMapper;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
import com.aifashionstudio.catalog.domain.exception.CatalogNameAlreadyExistsException;
import com.aifashionstudio.catalog.domain.exception.InvalidCatalogStatusTransitionException;
import com.aifashionstudio.catalog.domain.exception.InvalidCatalogUpdateException;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductImage;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductImageRepository;
import com.aifashionstudio.catalog.domain.service.CatalogDomainService;
import com.aifashionstudio.shared.exception.NotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
// dùng để tự động tạo constructor cho các trường final, giúp giảm boilerplate code
// @RequiredArgsConstructor sẽ tạo ra một constructor với tất cả các trường final của lớp
//  giúp dễ dàng khởi tạo các dependency thông qua constructor injection.
public class CatalogApplicationServiceImpl implements CatalogApplicationService {
    private final CatalogRepository catalogRepository;
    private final ProductImageRepository productImageRepository;
    private final CatalogDomainService catalogDomainService;
    private final CatalogApplicationMapper catalogApplicationMapper;

    @Override
    @Transactional
    public CatalogResult createCatalog(CreateCatalogCommand command) {
        catalogDomainService.validateCatalogCanBeCreated(command.name(), command.basePrice());

        Catalog catalog = Catalog.create(
                command.name(),
                command.description(),
                command.basePrice()
        );

        Catalog savedCatalog = catalogRepository.save(catalog);
        return catalogApplicationMapper.toResult(savedCatalog);
    }

    @Override
    @Transactional
    public CatalogResult updateCatalog(UpdateCatalogCommand command) {
        Catalog catalog = getCatalogOrThrow(command.id());
        String normalizedName = command.name() == null ? null : command.name().trim();

        if (normalizedName != null
                && catalogRepository.existsByNameIgnoreCaseAndIdNot(normalizedName, command.id())) {
            throw new CatalogNameAlreadyExistsException(normalizedName);
        }

        try {
            catalog.updateDetails(command.name(), command.description(), command.basePrice());
        } catch (IllegalArgumentException | IllegalStateException ex) {
            throw new InvalidCatalogUpdateException(ex.getMessage());
        }

        return catalogApplicationMapper.toResult(catalogRepository.save(catalog));
    }

    @Override
    @Transactional
    public CatalogResult changeCatalogStatus(ChangeCatalogStatusCommand command) {
        Catalog catalog = getCatalogOrThrow(command.id());

        try {
            catalog.changeStatus(command.status());
        } catch (IllegalArgumentException | IllegalStateException ex) {
            throw new InvalidCatalogStatusTransitionException(ex.getMessage());
        }

        return catalogApplicationMapper.toResult(catalogRepository.save(catalog));
    }

    @Override
    public CatalogResult getCatalogById(UUID id) {
        Catalog catalog = getCatalogOrThrow(id);

        return catalogApplicationMapper.toResult(catalog);
    }

    @Override
    public CatalogResult getPublicProductById(UUID id) {
        Catalog catalog = getCatalogOrThrow(id);
        if (catalog.getStatus() != CatalogStatus.ACTIVE) {
            throw new NotFoundException(
                    "PRODUCT_NOT_FOUND",
                    "Product not found with id: " + id
            );
        }

        return catalogApplicationMapper.toResult(catalog);
    }

    @Override
    public List<CatalogResult> getCatalogs() {
        return getCatalogs(null, null);
    }

    @Override
    public List<CatalogResult> getCatalogs(CatalogStatus status, String name) {
        return findCatalogs(status, name)
                .stream()
                .map(catalogApplicationMapper::toResult)
                .toList();
    }

    @Override
    @Transactional(readOnly = true)
    public List<CatalogResult> getPublicProducts(String name) {
        var products = findCatalogs(CatalogStatus.ACTIVE, name);
        var thumbnailUrls = getThumbnailUrls(products);

        return products
                .stream()
                .map(product -> catalogApplicationMapper.toResult(product, thumbnailUrls.get(product.getId())))
                .toList();
    }

    private Map<UUID, String> getThumbnailUrls(List<Catalog> products) {
        if (products.isEmpty()) {
            return Map.of();
        }

        var productIds = products.stream()
                .map(Catalog::getId)
                .toList();

        return productImageRepository
                .findByProductIdInOrderByProductIdAscThumbnailDescSortOrderAsc(productIds)
                .stream()
                .collect(Collectors.toMap(
                        image -> image.getProduct().getId(),
                        ProductImage::getImageUrl,
                        (existing, ignored) -> existing
                ));
    }

    private Catalog getCatalogOrThrow(UUID id) {
        return catalogRepository.findById(id)
                .orElseThrow(() -> new NotFoundException(
                        "CATALOG_NOT_FOUND",
                        "Catalog not found with id: " + id
                ));
    }

    private List<Catalog> findCatalogs(CatalogStatus status, String name) {
        String normalizedName = name == null ? null : name.trim();

        if (status != null && normalizedName != null && !normalizedName.isBlank()) {
            return catalogRepository.findByStatusAndNameContainingIgnoreCase(status, normalizedName);
        }
        if (status != null) {
            return catalogRepository.findByStatus(status);
        }
        if (normalizedName != null && !normalizedName.isBlank()) {
            return catalogRepository.findByNameContainingIgnoreCase(normalizedName);
        }
        return catalogRepository.findAll();
    }
}

package com.aifashionstudio.catalog.application.service.impl;

import com.aifashionstudio.catalog.application.command.CreateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import com.aifashionstudio.catalog.application.mapper.CatalogApplicationMapper;
import com.aifashionstudio.catalog.application.service.CatalogApplicationService;
import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.service.CatalogDomainService;
import com.aifashionstudio.shared.exception.NotFoundException;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
// dùng để tự động tạo constructor cho các trường final, giúp giảm boilerplate code
// @RequiredArgsConstructor sẽ tạo ra một constructor với tất cả các trường final của lớp
//  giúp dễ dàng khởi tạo các dependency thông qua constructor injection.
public class CatalogApplicationServiceImpl implements CatalogApplicationService {
    private final CatalogRepository catalogRepository;
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
    public CatalogResult getCatalogById(UUID id) {
        Catalog catalog = catalogRepository.findById(id)
                .orElseThrow(() -> new NotFoundException(
                        "CATALOG_NOT_FOUND",
                        "Catalog not found with id: " + id
                ));

        return catalogApplicationMapper.toResult(catalog);
    }

    @Override
    public List<CatalogResult> getCatalogs() {
        return catalogRepository.findAll()
                .stream().map(catalogApplicationMapper::toResult).toList();
    }
}

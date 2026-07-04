package com.aifashionstudio.catalog.api.mapper;

import com.aifashionstudio.catalog.api.dto.CatalogResponse;
import com.aifashionstudio.catalog.api.dto.CreateCatalogRequest;
import com.aifashionstudio.catalog.application.command.CreateCatalogCommand;
import com.aifashionstudio.catalog.application.dto.CatalogResult;
import org.springframework.stereotype.Component;

@Component
public class CatalogApiMapper {
    // class dùng để mapper giữa request và command, result và response
    // nó là sự liên kết map từ application layer sang api layer và ngược lại
    // cái này giúp tách biệt các lớp và giữ cho code sạch sẽ, dễ bảo trì và mở rộng.
    // cái result nó là cái mà application layer trả về,
    // còn response là cái mà api layer trả về cho client

    public CreateCatalogCommand toCommand(CreateCatalogRequest request) {
        return new CreateCatalogCommand(
                request.name(),
                request.description(),
                request.basePrice()
        );
    }

    public CatalogResponse toResponse(CatalogResult result) {
        return new CatalogResponse(
                result.id(),
                result.name(),
                result.description(),
                result.basePrice(),
                result.status(),
                result.createdAt(),
                result.updatedAt()
        );
    }
}

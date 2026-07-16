package com.aifashionstudio.catalog.application.service;

import com.aifashionstudio.catalog.application.dto.ProductDetailResult;

import java.util.UUID;

public interface ProductDetailApplicationService {

    ProductDetailResult getPublicProductDetail(UUID productId);
}

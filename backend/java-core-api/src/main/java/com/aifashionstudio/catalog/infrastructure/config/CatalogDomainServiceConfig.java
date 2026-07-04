package com.aifashionstudio.catalog.infrastructure.config;

import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.service.CatalogDomainService;
import com.aifashionstudio.catalog.domain.service.impl.CatalogDomainServiceImpl;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration class for CatalogDomainService.
 * class này được sử dụng để cấu hình và tạo bean cho CatalogDomainService trong ứng dụng Spring Boot.
 * để tránh việc tạo bean trực tiếp trong các lớp khác, giúp tách biệt logic cấu hình và logic nghiệp vụ.
 */
@Configuration
public class CatalogDomainServiceConfig {

    @Bean
    public CatalogDomainService catalogDomainService(CatalogRepository catalogRepository) {
        return new CatalogDomainServiceImpl(catalogRepository);
    }
}

package com.aifashionstudio.shared.config;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Info;
import io.swagger.v3.oas.models.servers.Server;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.util.List;

@Configuration
public class OpenApiConfig {
    @Bean
    public OpenAPI javaCoreOpenAPI() {
        return new OpenAPI()
                .servers(List.of(new Server()
                        .url("/")
                        .description("API gateway")))
                .info(new Info()
                        .title("AI Fashion Studio - Java Core API")
                        .version("v1")
                        .description("Catalog, design, try-on, ordering and feedback APIs"));
    }
}

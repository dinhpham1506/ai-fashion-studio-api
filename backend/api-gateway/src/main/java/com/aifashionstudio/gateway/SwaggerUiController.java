package com.aifashionstudio.gateway;

import java.util.List;
import java.util.Map;

import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class SwaggerUiController {

    @GetMapping(value = "/webjars/swagger-ui/swagger-initializer.js", produces = "application/javascript")
    public String swaggerInitializer() {
        return """
                window.onload = function() {
                  window.ui = SwaggerUIBundle({
                    configUrl: "/v3/api-docs/swagger-config",
                    dom_id: "#swagger-ui",
                    deepLinking: true,
                    presets: [
                      SwaggerUIBundle.presets.apis,
                      SwaggerUIStandalonePreset
                    ],
                    plugins: [
                      SwaggerUIBundle.plugins.DownloadUrl
                    ],
                    layout: "StandaloneLayout"
                  });
                };
                """;
    }

    @GetMapping(value = "/v3/api-docs/swagger-config", produces = MediaType.APPLICATION_JSON_VALUE)
    public Map<String, Object> swaggerConfig() {
        return Map.of(
                "configUrl", "/v3/api-docs/swagger-config",
                "urls", List.of(
                        Map.of("name", "Java Core API", "url", "/v3/api-docs/java-core-api"),
                        Map.of("name", "Platform API", "url", "/v3/api-docs/platform-api")),
                "validatorUrl", "");
    }
}

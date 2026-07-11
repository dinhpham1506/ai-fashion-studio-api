package com.aifashionstudio.design.domain.model;

import com.aifashionstudio.shared.domain.model.common.AuditableDomainModel;

import java.time.OffsetDateTime;
import java.util.Map;
import java.util.UUID;

public class Design extends AuditableDomainModel {

    private UUID customerId;
    private UUID productId;
    private UUID productVariantId;
    private String name;
    private Map<String, Object> canvasJson;
    private String previewImageUrl;
    private String printFileUrl;
    private DesignStatus status;

    protected Design() {
    }

    public static Design createDraft(UUID customerId, UUID productId, UUID productVariantId, String name) {
        validateRequired(customerId, "Customer id cannot be null");
        validateRequired(productId, "Product id cannot be null");
        validateRequired(productVariantId, "Product variant id cannot be null");
        validateName(name);

        Design design = new Design();
        design.customerId = customerId;
        design.productId = productId;
        design.productVariantId = productVariantId;
        design.name = name.trim();
        design.canvasJson = Map.of();
        design.status = DesignStatus.DRAFT;
        return design;
    }

    public static Design reconstitute(UUID id,
                                      UUID customerId,
                                      UUID productId,
                                      UUID productVariantId,
                                      String name,
                                      Map<String, Object> canvasJson,
                                      String previewImageUrl,
                                      String printFileUrl,
                                      DesignStatus status,
                                      OffsetDateTime createdAt,
                                      OffsetDateTime updatedAt) {
        Design design = new Design();
        design.setId(id);
        design.customerId = customerId;
        design.productId = productId;
        design.productVariantId = productVariantId;
        design.name = name;
        design.canvasJson = canvasJson == null ? Map.of() : Map.copyOf(canvasJson);
        design.previewImageUrl = previewImageUrl;
        design.printFileUrl = printFileUrl;
        design.status = status;
        design.setCreatedAt(createdAt);
        design.setUpdatedAt(updatedAt);
        return design;
    }

    public void save(String name, Map<String, Object> canvasJson, String previewImageUrl, String printFileUrl) {
        if (status == DesignStatus.LOCKED) {
            throw new IllegalStateException("Design is locked");
        }
        validateName(name);
        if (canvasJson == null) {
            throw new IllegalArgumentException("Canvas json cannot be null");
        }

        this.name = name.trim();
        this.canvasJson = Map.copyOf(canvasJson);
        this.previewImageUrl = normalize(previewImageUrl);
        this.printFileUrl = normalize(printFileUrl);
        this.status = DesignStatus.SAVED;
    }

    public void lock() {
        if (status != DesignStatus.SAVED && status != DesignStatus.LOCKED) {
            throw new IllegalStateException("Only saved designs can be locked");
        }
        this.status = DesignStatus.LOCKED;
    }

    public UUID getCustomerId() {
        return customerId;
    }

    public UUID getProductId() {
        return productId;
    }

    public UUID getProductVariantId() {
        return productVariantId;
    }

    public String getName() {
        return name;
    }

    public Map<String, Object> getCanvasJson() {
        return canvasJson;
    }

    public String getPreviewImageUrl() {
        return previewImageUrl;
    }

    public String getPrintFileUrl() {
        return printFileUrl;
    }

    public DesignStatus getStatus() {
        return status;
    }

    private static void validateRequired(UUID value, String message) {
        if (value == null) {
            throw new IllegalArgumentException(message);
        }
    }

    private static void validateName(String name) {
        if (name == null || name.isBlank()) {
            throw new IllegalArgumentException("Design name cannot be null or empty");
        }
    }

    private static String normalize(String value) {
        return value == null || value.isBlank() ? null : value.trim();
    }
}

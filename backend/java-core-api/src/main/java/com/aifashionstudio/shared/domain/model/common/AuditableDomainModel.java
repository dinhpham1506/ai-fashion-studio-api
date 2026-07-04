package com.aifashionstudio.shared.domain.model.common;

import java.time.OffsetDateTime;
import java.util.UUID;

public abstract class AuditableDomainModel extends BaseDomainModel {

    private OffsetDateTime createdAt;

    private OffsetDateTime updatedAt;

    protected AuditableDomainModel() {
    }

    protected AuditableDomainModel(UUID id, OffsetDateTime createdAt, OffsetDateTime updatedAt) {
        super(id);
        this.createdAt = createdAt;
        this.updatedAt = updatedAt;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public OffsetDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(OffsetDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }
}

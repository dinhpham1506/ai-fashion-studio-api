package com.aifashionstudio.shared.domain.model.common;

import java.util.UUID;

public abstract class BaseDomainModel {

    private UUID id;

    protected BaseDomainModel() {
    }

    protected BaseDomainModel(UUID id) {
        this.id = id;
    }

    public UUID getId() {
        return id;
    }

    public void setId(UUID id) {
        this.id = id;
    }
}

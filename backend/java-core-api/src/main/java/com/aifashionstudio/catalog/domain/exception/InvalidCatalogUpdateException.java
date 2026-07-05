package com.aifashionstudio.catalog.domain.exception;

import com.aifashionstudio.shared.exception.BusinessRuleException;

public class InvalidCatalogUpdateException extends BusinessRuleException {
    public InvalidCatalogUpdateException(String message) {
        super("INVALID_CATALOG_UPDATE", message);
    }
}

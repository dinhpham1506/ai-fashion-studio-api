package com.aifashionstudio.catalog.domain.exception;

import com.aifashionstudio.shared.exception.BusinessRuleException;

public class InvalidCatalogStatusTransitionException extends BusinessRuleException {
    public InvalidCatalogStatusTransitionException(String message) {
        super("INVALID_CATALOG_STATUS_TRANSITION", message);
    }
}

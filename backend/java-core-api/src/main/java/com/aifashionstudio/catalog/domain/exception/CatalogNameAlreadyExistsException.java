package com.aifashionstudio.catalog.domain.exception;

import com.aifashionstudio.shared.exception.ConflictException;

public class CatalogNameAlreadyExistsException extends ConflictException {
  // bắt exception ở đây để tránh trùng tên catalog
  // và trả về thông báo lỗi cho người dùng

    public CatalogNameAlreadyExistsException(String message) {
        super("CATALOG_NAME_ALREADY_EXISTS",
                "Catalog with the same same already exists: " + message);
    }
}

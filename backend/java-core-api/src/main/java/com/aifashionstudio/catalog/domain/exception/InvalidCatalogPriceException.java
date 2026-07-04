package com.aifashionstudio.catalog.domain.exception;

import com.aifashionstudio.shared.exception.BusinessRuleException;

public class InvalidCatalogPriceException extends BusinessRuleException {
  // bắt exception kế thừa từ BusinessRuleException để tránh giá trị catalog không hợp lệ
  // ngoài ra còn có thể thêm các thông tin khác như mã lỗi,
  // thông tin chi tiết về giá trị không hợp lệ, v.v.

    public InvalidCatalogPriceException(String message) {
        super("INVALID_CATALOG_PRICE","Base price cannot be null or negative" + message);
    }
}

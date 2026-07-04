package com.aifashionstudio.catalog.application.command;

import java.math.BigDecimal;
// record này được sử dụng để tạo một lệnh (command) trong ứng dụng,
//đại diện cho việc tạo một catalog mới với các thuộc tính như tên, mô tả và giá cơ bản.
// thể tính bất biến không có setter, chỉ có getter và constructor tự động được tạo ra bởi Java.
public record CreateCatalogCommand(String name,
                                   String description,
                                   BigDecimal basePrice) {
}

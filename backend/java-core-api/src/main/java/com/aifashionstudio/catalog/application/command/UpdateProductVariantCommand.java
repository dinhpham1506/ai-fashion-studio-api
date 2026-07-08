package com.aifashionstudio.catalog.application.command;

import java.math.BigDecimal;

public record UpdateProductVariantCommand(
        String sku,
        String size,
        String color,
        String material,
        BigDecimal priceAdjustment
) {
}

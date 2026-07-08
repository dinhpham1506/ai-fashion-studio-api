package com.aifashionstudio.catalog.application.command;

public record CreateProductImageCommand(
        String imageUrl,
        boolean thumbnail,
        int sortOrder
) {
}

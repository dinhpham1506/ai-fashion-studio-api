package com.aifashionstudio.catalog.application.command;

public record UpdateProductImageCommand(
        String imageUrl,
        boolean thumbnail,
        int sortOrder
) {
}

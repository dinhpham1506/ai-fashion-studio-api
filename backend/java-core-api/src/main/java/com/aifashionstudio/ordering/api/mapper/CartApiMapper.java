package com.aifashionstudio.ordering.api.mapper;

import com.aifashionstudio.ordering.api.dto.AddCartItemRequest;
import com.aifashionstudio.ordering.api.dto.CartItemResponse;
import com.aifashionstudio.ordering.api.dto.CartResponse;
import com.aifashionstudio.ordering.api.dto.CheckoutCartRequest;
import com.aifashionstudio.ordering.api.dto.UpdateCartItemRequest;
import com.aifashionstudio.ordering.application.command.AddCartItemCommand;
import com.aifashionstudio.ordering.application.command.CheckoutCartCommand;
import com.aifashionstudio.ordering.application.command.UpdateCartItemCommand;
import com.aifashionstudio.ordering.application.dto.CartItemResult;
import com.aifashionstudio.ordering.application.dto.CartResult;
import org.springframework.stereotype.Component;

import java.util.UUID;

@Component
public class CartApiMapper {

    public AddCartItemCommand toCommand(UUID customerId, AddCartItemRequest request) {
        return new AddCartItemCommand(
                customerId,
                request.productId(),
                request.productVariantId(),
                request.designId(),
                request.quantity()
        );
    }

    public UpdateCartItemCommand toCommand(UUID customerId, UUID itemId, UpdateCartItemRequest request) {
        return new UpdateCartItemCommand(customerId, itemId, request.quantity());
    }

    public CheckoutCartCommand toCommand(UUID customerId, CheckoutCartRequest request) {
        return new CheckoutCartCommand(
                customerId,
                request.receiverName(),
                request.receiverPhone(),
                request.shippingAddress()
        );
    }

    public CartResponse toResponse(CartResult result) {
        return new CartResponse(
                result.id(),
                result.customerId(),
                result.items().stream()
                        .map(this::toResponse)
                        .toList(),
                result.totalQuantity(),
                result.totalAmount()
        );
    }

    private CartItemResponse toResponse(CartItemResult result) {
        return new CartItemResponse(
                result.id(),
                result.productId(),
                result.productName(),
                result.productVariantId(),
                result.sku(),
                result.size(),
                result.color(),
                result.material(),
                result.designId(),
                result.designName(),
                result.previewImageUrl(),
                result.quantity(),
                result.availableQuantity(),
                result.unitPrice(),
                result.totalPrice()
        );
    }
}

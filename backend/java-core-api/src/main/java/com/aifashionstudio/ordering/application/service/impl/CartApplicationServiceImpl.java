package com.aifashionstudio.ordering.application.service.impl;

import com.aifashionstudio.catalog.domain.model.Catalog;
import com.aifashionstudio.catalog.domain.model.CatalogStatus;
import com.aifashionstudio.catalog.domain.model.ProductInventory;
import com.aifashionstudio.catalog.domain.model.ProductVariant;
import com.aifashionstudio.catalog.domain.model.ProductVariantStatus;
import com.aifashionstudio.catalog.domain.repository.CatalogRepository;
import com.aifashionstudio.catalog.domain.repository.ProductInventoryRepository;
import com.aifashionstudio.catalog.domain.repository.ProductVariantRepository;
import com.aifashionstudio.design.domain.model.Design;
import com.aifashionstudio.design.domain.model.DesignStatus;
import com.aifashionstudio.design.domain.repository.DesignRepository;
import com.aifashionstudio.ordering.application.command.AddCartItemCommand;
import com.aifashionstudio.ordering.application.command.CheckoutCartCommand;
import com.aifashionstudio.ordering.application.command.CreateOrderCommand;
import com.aifashionstudio.ordering.application.command.UpdateCartItemCommand;
import com.aifashionstudio.ordering.application.dto.CartItemResult;
import com.aifashionstudio.ordering.application.dto.CartResult;
import com.aifashionstudio.ordering.application.dto.OrderCreatedResult;
import com.aifashionstudio.ordering.application.service.CartApplicationService;
import com.aifashionstudio.ordering.application.service.OrderApplicationService;
import com.aifashionstudio.ordering.domain.model.Cart;
import com.aifashionstudio.ordering.domain.model.CartItem;
import com.aifashionstudio.ordering.domain.repository.CartRepository;
import com.aifashionstudio.shared.exception.BusinessRuleException;
import com.aifashionstudio.shared.exception.ForbiddenException;
import com.aifashionstudio.shared.exception.NotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class CartApplicationServiceImpl implements CartApplicationService {

    private final CartRepository cartRepository;
    private final CatalogRepository catalogRepository;
    private final ProductVariantRepository productVariantRepository;
    private final ProductInventoryRepository productInventoryRepository;
    private final DesignRepository designRepository;
    private final OrderApplicationService orderApplicationService;

    @Override
    public CartResult getCart(UUID customerId) {
        return toResult(getOrCreateCart(customerId));
    }

    @Override
    @Transactional
    public CartResult addItem(AddCartItemCommand command) {
        validateCartItem(command.customerId(), command.productId(), command.productVariantId(), command.designId(), command.quantity());

        Cart cart = getOrCreateCart(command.customerId());
        cart.addOrIncreaseItem(CartItem.create(
                command.productId(),
                command.productVariantId(),
                command.designId(),
                command.quantity()
        ));
        validateCartQuantities(cart);

        return toResult(cartRepository.save(cart));
    }

    @Override
    @Transactional
    public CartResult updateItem(UpdateCartItemCommand command) {
        Cart cart = getExistingCart(command.customerId());
        CartItem item = findCartItem(cart, command.cartItemId());
        validateCartItem(command.customerId(), item.getProductId(), item.getProductVariantId(), item.getDesignId(), command.quantity());
        cart.updateItemQuantity(command.cartItemId(), command.quantity());
        validateCartQuantities(cart);

        return toResult(cartRepository.save(cart));
    }

    @Override
    @Transactional
    public CartResult removeItem(UUID customerId, UUID cartItemId) {
        Cart cart = getExistingCart(customerId);
        findCartItem(cart, cartItemId);
        cart.removeItem(cartItemId);

        return toResult(cartRepository.save(cart));
    }

    @Override
    @Transactional
    public CartResult clearCart(UUID customerId) {
        Cart cart = getOrCreateCart(customerId);
        cart.clear();
        return toResult(cartRepository.save(cart));
    }

    @Override
    @Transactional
    public OrderCreatedResult checkout(CheckoutCartCommand command) {
        Cart cart = getExistingCart(command.customerId());
        if (cart.getItems().isEmpty()) {
            throw new BusinessRuleException("CART_EMPTY", "Cart is empty");
        }
        validateCartQuantities(cart);

        OrderCreatedResult result = orderApplicationService.createOrder(new CreateOrderCommand(
                command.customerId(),
                cart.getItems().stream()
                        .map(item -> new CreateOrderCommand.CreateOrderItemCommand(
                                item.getProductId(),
                                item.getProductVariantId(),
                                item.getDesignId(),
                                item.getQuantity()
                        ))
                        .toList(),
                command.receiverName(),
                command.receiverPhone(),
                command.shippingAddress()
        ));

        cart.clear();
        cartRepository.save(cart);
        return result;
    }

    private Cart getOrCreateCart(UUID customerId) {
        return cartRepository.findByCustomerId(customerId)
                .orElseGet(() -> cartRepository.save(Cart.create(customerId)));
    }

    private Cart getExistingCart(UUID customerId) {
        return cartRepository.findByCustomerId(customerId)
                .orElseThrow(() -> new NotFoundException("CART_NOT_FOUND", "Cart not found"));
    }

    private CartItem findCartItem(Cart cart, UUID cartItemId) {
        try {
            return cart.findItem(cartItemId);
        } catch (IllegalArgumentException ex) {
            throw new NotFoundException("CART_ITEM_NOT_FOUND", "Cart item not found with id: " + cartItemId);
        }
    }

    private void validateCartItem(UUID customerId, UUID productId, UUID productVariantId, UUID designId, int quantity) {
        if (quantity <= 0) {
            throw new BusinessRuleException("INVALID_QUANTITY", "Quantity must be greater than zero");
        }

        Catalog product = catalogRepository.findById(productId)
                .orElseThrow(() -> new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + productId));
        if (product.getStatus() != CatalogStatus.ACTIVE) {
            throw new BusinessRuleException("PRODUCT_NOT_AVAILABLE", "Product is not available");
        }

        ProductVariant variant = productVariantRepository.findById(productVariantId)
                .orElseThrow(() -> new NotFoundException("VARIANT_NOT_FOUND", "Variant not found with id: " + productVariantId));
        if (!variant.getProduct().getId().equals(product.getId()) || variant.getStatus() != ProductVariantStatus.ACTIVE) {
            throw new BusinessRuleException("VARIANT_NOT_AVAILABLE", "Variant is not available");
        }

        Design design = designRepository.findById(designId)
                .orElseThrow(() -> new NotFoundException("DESIGN_NOT_FOUND", "Design not found with id: " + designId));
        if (!design.getCustomerId().equals(customerId)) {
            throw new ForbiddenException("DESIGN_ACCESS_DENIED", "Design access denied");
        }
        if (design.getStatus() != DesignStatus.SAVED) {
            throw new BusinessRuleException("DESIGN_MUST_BE_SAVED", "Design must be saved before adding to cart");
        }
        if (!design.getProductId().equals(product.getId()) || !design.getProductVariantId().equals(variant.getId())) {
            throw new BusinessRuleException("DESIGN_PRODUCT_MISMATCH", "Design does not match requested product variant");
        }
    }

    private void validateCartQuantities(Cart cart) {
        for (CartItem item : cart.getItems()) {
            ProductInventory inventory = getInventory(item.getProductVariantId());
            if (inventory.getAvailableQuantity() < item.getQuantity()) {
                throw new BusinessRuleException("PRODUCT_OUT_OF_STOCK", "Product is out of stock");
            }
        }
    }

    private CartResult toResult(Cart cart) {
        List<CartItemResult> items = cart.getItems().stream()
                .map(this::toItemResult)
                .toList();
        BigDecimal totalAmount = items.stream()
                .map(CartItemResult::totalPrice)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
        int totalQuantity = items.stream()
                .mapToInt(CartItemResult::quantity)
                .sum();

        return new CartResult(cart.getId(), cart.getCustomerId(), items, totalQuantity, totalAmount);
    }

    private CartItemResult toItemResult(CartItem item) {
        Catalog product = catalogRepository.findById(item.getProductId())
                .orElseThrow(() -> new NotFoundException("PRODUCT_NOT_FOUND", "Product not found with id: " + item.getProductId()));
        ProductVariant variant = productVariantRepository.findById(item.getProductVariantId())
                .orElseThrow(() -> new NotFoundException("VARIANT_NOT_FOUND", "Variant not found with id: " + item.getProductVariantId()));
        ProductInventory inventory = getInventory(item.getProductVariantId());
        Design design = designRepository.findById(item.getDesignId())
                .orElseThrow(() -> new NotFoundException("DESIGN_NOT_FOUND", "Design not found with id: " + item.getDesignId()));

        BigDecimal unitPrice = product.getBasePrice().add(variant.getPriceAdjustment());
        return new CartItemResult(
                item.getId(),
                product.getId(),
                product.getName(),
                variant.getId(),
                variant.getSku(),
                variant.getSize(),
                variant.getColor(),
                variant.getMaterial(),
                design.getId(),
                design.getName(),
                design.getPreviewImageUrl(),
                item.getQuantity(),
                inventory.getAvailableQuantity(),
                unitPrice,
                unitPrice.multiply(BigDecimal.valueOf(item.getQuantity()))
        );
    }

    private ProductInventory getInventory(UUID productVariantId) {
        return productInventoryRepository.findByProductVariantId(productVariantId)
                .orElseThrow(() -> new BusinessRuleException("PRODUCT_OUT_OF_STOCK", "Product inventory not found"));
    }
}

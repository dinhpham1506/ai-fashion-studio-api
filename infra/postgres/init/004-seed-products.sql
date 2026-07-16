-- ============================================================
-- Demo product catalog seed
-- Idempotent: safe to run multiple times.
-- ============================================================

WITH demo_products (id, name, description, base_price, status) AS (
  VALUES
    (
      '11111111-1111-4111-8111-111111111111'::uuid,
      'Classic Cotton T-Shirt',
      'Soft everyday cotton tee for custom AI prints and casual wear.',
      199000.00,
      'ACTIVE'
    ),
    (
      '22222222-2222-4222-8222-222222222222'::uuid,
      'Oversized Streetwear Tee',
      'Relaxed oversized silhouette with heavyweight fabric and a clean front panel for design work.',
      249000.00,
      'ACTIVE'
    ),
    (
      '33333333-3333-4333-8333-333333333333'::uuid,
      'Premium Hoodie',
      'Warm fleece hoodie with a roomy fit, kangaroo pocket, and print-ready surface.',
      499000.00,
      'ACTIVE'
    ),
    (
      '44444444-4444-4444-8444-444444444444'::uuid,
      'Canvas Tote Bag',
      'Durable reusable canvas tote for custom artwork, branding, and daily carry.',
      159000.00,
      'ACTIVE'
    ),
    (
      '55555555-5555-4555-8555-555555555555'::uuid,
      'Athletic Performance Tee',
      'Lightweight quick-dry performance shirt built for active custom collections.',
      229000.00,
      'ACTIVE'
    ),
    (
      '66666666-6666-4666-8666-666666666666'::uuid,
      'Long Sleeve Layer Tee',
      'Comfortable long sleeve tee with a modern regular fit and smooth printable texture.',
      279000.00,
      'ACTIVE'
    )
)
INSERT INTO catalog.products (id, name, description, base_price, status)
SELECT id, name, description, base_price, status
FROM demo_products
ON CONFLICT (id) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    base_price = EXCLUDED.base_price,
    status = EXCLUDED.status,
    updated_at = CURRENT_TIMESTAMP;

WITH demo_images (id, product_id, image_url, is_thumbnail, sort_order) AS (
  VALUES
    ('11111111-aaaa-4111-8111-111111111111'::uuid, '11111111-1111-4111-8111-111111111111'::uuid, 'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=1200&q=80', true, 0),
    ('11111111-aaab-4111-8111-111111111111'::uuid, '11111111-1111-4111-8111-111111111111'::uuid, 'https://images.unsplash.com/photo-1583743814966-8936f5b7be1a?auto=format&fit=crop&w=1200&q=80', false, 1),
    ('22222222-aaaa-4222-8222-222222222222'::uuid, '22222222-2222-4222-8222-222222222222'::uuid, 'https://images.unsplash.com/photo-1576566588028-4147f3842f27?auto=format&fit=crop&w=1200&q=80', true, 0),
    ('22222222-aaab-4222-8222-222222222222'::uuid, '22222222-2222-4222-8222-222222222222'::uuid, 'https://images.unsplash.com/photo-1503342217505-b0a15ec3261c?auto=format&fit=crop&w=1200&q=80', false, 1),
    ('33333333-aaaa-4333-8333-333333333333'::uuid, '33333333-3333-4333-8333-333333333333'::uuid, 'https://images.unsplash.com/photo-1556821840-3a63f95609a7?auto=format&fit=crop&w=1200&q=80', true, 0),
    ('33333333-aaab-4333-8333-333333333333'::uuid, '33333333-3333-4333-8333-333333333333'::uuid, 'https://images.unsplash.com/photo-1578681994506-b8f463449011?auto=format&fit=crop&w=1200&q=80', false, 1),
    ('44444444-aaaa-4444-8444-444444444444'::uuid, '44444444-4444-4444-8444-444444444444'::uuid, 'https://images.unsplash.com/photo-1590874103328-eac38a683ce7?auto=format&fit=crop&w=1200&q=80', true, 0),
    ('44444444-aaab-4444-8444-444444444444'::uuid, '44444444-4444-4444-8444-444444444444'::uuid, 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80', false, 1),
    ('55555555-aaaa-4555-8555-555555555555'::uuid, '55555555-5555-4555-8555-555555555555'::uuid, 'https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=1200&q=80', true, 0),
    ('55555555-aaab-4555-8555-555555555555'::uuid, '55555555-5555-4555-8555-555555555555'::uuid, 'https://images.unsplash.com/photo-1516257984-b1b4d707412e?auto=format&fit=crop&w=1200&q=80', false, 1),
    ('66666666-aaaa-4666-8666-666666666666'::uuid, '66666666-6666-4666-8666-666666666666'::uuid, 'https://images.unsplash.com/photo-1618354691373-d851c5c3a990?auto=format&fit=crop&w=1200&q=80', true, 0),
    ('66666666-aaab-4666-8666-666666666666'::uuid, '66666666-6666-4666-8666-666666666666'::uuid, 'https://images.unsplash.com/photo-1618354691438-25bc04584c23?auto=format&fit=crop&w=1200&q=80', false, 1)
)
INSERT INTO catalog.product_images (id, product_id, image_url, is_thumbnail, sort_order)
SELECT id, product_id, image_url, is_thumbnail, sort_order
FROM demo_images
ON CONFLICT (id) DO UPDATE
SET image_url = EXCLUDED.image_url,
    is_thumbnail = EXCLUDED.is_thumbnail,
    sort_order = EXCLUDED.sort_order;

WITH demo_variants (id, product_id, sku, size, color, material, price_adjustment, status) AS (
  VALUES
    ('11111111-b111-4111-8111-111111111111'::uuid, '11111111-1111-4111-8111-111111111111'::uuid, 'AFS-TEE-CLASSIC-WHT-S', 'S', 'White', 'Cotton', 0.00, 'ACTIVE'),
    ('11111111-b112-4111-8111-111111111111'::uuid, '11111111-1111-4111-8111-111111111111'::uuid, 'AFS-TEE-CLASSIC-WHT-M', 'M', 'White', 'Cotton', 0.00, 'ACTIVE'),
    ('11111111-b113-4111-8111-111111111111'::uuid, '11111111-1111-4111-8111-111111111111'::uuid, 'AFS-TEE-CLASSIC-BLK-L', 'L', 'Black', 'Cotton', 15000.00, 'ACTIVE'),
    ('22222222-b111-4222-8222-222222222222'::uuid, '22222222-2222-4222-8222-222222222222'::uuid, 'AFS-TEE-OVERSIZE-BLK-M', 'M', 'Black', 'Heavy Cotton', 0.00, 'ACTIVE'),
    ('22222222-b112-4222-8222-222222222222'::uuid, '22222222-2222-4222-8222-222222222222'::uuid, 'AFS-TEE-OVERSIZE-BLK-L', 'L', 'Black', 'Heavy Cotton', 10000.00, 'ACTIVE'),
    ('22222222-b113-4222-8222-222222222222'::uuid, '22222222-2222-4222-8222-222222222222'::uuid, 'AFS-TEE-OVERSIZE-GRY-XL', 'XL', 'Grey', 'Heavy Cotton', 20000.00, 'ACTIVE'),
    ('33333333-b111-4333-8333-333333333333'::uuid, '33333333-3333-4333-8333-333333333333'::uuid, 'AFS-HOODIE-PRM-BLK-M', 'M', 'Black', 'Fleece', 0.00, 'ACTIVE'),
    ('33333333-b112-4333-8333-333333333333'::uuid, '33333333-3333-4333-8333-333333333333'::uuid, 'AFS-HOODIE-PRM-BLK-L', 'L', 'Black', 'Fleece', 20000.00, 'ACTIVE'),
    ('33333333-b113-4333-8333-333333333333'::uuid, '33333333-3333-4333-8333-333333333333'::uuid, 'AFS-HOODIE-PRM-CREAM-M', 'M', 'Cream', 'Fleece', 10000.00, 'ACTIVE'),
    ('44444444-b111-4444-8444-444444444444'::uuid, '44444444-4444-4444-8444-444444444444'::uuid, 'AFS-TOTE-CANVAS-NAT-STD', 'Standard', 'Natural', 'Canvas', 0.00, 'ACTIVE'),
    ('44444444-b112-4444-8444-444444444444'::uuid, '44444444-4444-4444-8444-444444444444'::uuid, 'AFS-TOTE-CANVAS-BLK-STD', 'Standard', 'Black', 'Canvas', 15000.00, 'ACTIVE'),
    ('55555555-b111-4555-8555-555555555555'::uuid, '55555555-5555-4555-8555-555555555555'::uuid, 'AFS-PERF-TEE-NAVY-M', 'M', 'Navy', 'Polyester', 0.00, 'ACTIVE'),
    ('55555555-b112-4555-8555-555555555555'::uuid, '55555555-5555-4555-8555-555555555555'::uuid, 'AFS-PERF-TEE-NAVY-L', 'L', 'Navy', 'Polyester', 10000.00, 'ACTIVE'),
    ('55555555-b113-4555-8555-555555555555'::uuid, '55555555-5555-4555-8555-555555555555'::uuid, 'AFS-PERF-TEE-RED-M', 'M', 'Red', 'Polyester', 10000.00, 'ACTIVE'),
    ('66666666-b111-4666-8666-666666666666'::uuid, '66666666-6666-4666-8666-666666666666'::uuid, 'AFS-LS-TEE-WHT-M', 'M', 'White', 'Cotton Blend', 0.00, 'ACTIVE'),
    ('66666666-b112-4666-8666-666666666666'::uuid, '66666666-6666-4666-8666-666666666666'::uuid, 'AFS-LS-TEE-BLK-L', 'L', 'Black', 'Cotton Blend', 10000.00, 'ACTIVE')
)
INSERT INTO catalog.product_variants (id, product_id, sku, size, color, material, price_adjustment, status)
SELECT id, product_id, sku, size, color, material, price_adjustment, status
FROM demo_variants
ON CONFLICT (sku) DO UPDATE
SET product_id = EXCLUDED.product_id,
    size = EXCLUDED.size,
    color = EXCLUDED.color,
    material = EXCLUDED.material,
    price_adjustment = EXCLUDED.price_adjustment,
    status = EXCLUDED.status,
    updated_at = CURRENT_TIMESTAMP;

WITH demo_inventory (product_variant_id, available_quantity, reserved_quantity, sold_quantity) AS (
  VALUES
    ('11111111-b111-4111-8111-111111111111'::uuid, 40, 0, 8),
    ('11111111-b112-4111-8111-111111111111'::uuid, 55, 2, 14),
    ('11111111-b113-4111-8111-111111111111'::uuid, 35, 1, 10),
    ('22222222-b111-4222-8222-222222222222'::uuid, 32, 0, 6),
    ('22222222-b112-4222-8222-222222222222'::uuid, 28, 1, 9),
    ('22222222-b113-4222-8222-222222222222'::uuid, 18, 0, 3),
    ('33333333-b111-4333-8333-333333333333'::uuid, 24, 1, 4),
    ('33333333-b112-4333-8333-333333333333'::uuid, 20, 0, 5),
    ('33333333-b113-4333-8333-333333333333'::uuid, 16, 0, 2),
    ('44444444-b111-4444-8444-444444444444'::uuid, 80, 3, 20),
    ('44444444-b112-4444-8444-444444444444'::uuid, 45, 1, 11),
    ('55555555-b111-4555-8555-555555555555'::uuid, 42, 0, 7),
    ('55555555-b112-4555-8555-555555555555'::uuid, 36, 2, 6),
    ('55555555-b113-4555-8555-555555555555'::uuid, 30, 0, 4),
    ('66666666-b111-4666-8666-666666666666'::uuid, 34, 1, 5),
    ('66666666-b112-4666-8666-666666666666'::uuid, 29, 0, 6)
)
INSERT INTO catalog.product_inventory (product_variant_id, available_quantity, reserved_quantity, sold_quantity)
SELECT product_variant_id, available_quantity, reserved_quantity, sold_quantity
FROM demo_inventory
ON CONFLICT (product_variant_id) DO UPDATE
SET available_quantity = EXCLUDED.available_quantity,
    reserved_quantity = EXCLUDED.reserved_quantity,
    sold_quantity = EXCLUDED.sold_quantity,
    updated_at = CURRENT_TIMESTAMP;

-- Keep every ACTIVE product variant buyable for local FE demos, including
-- products/variants created outside this seed file.
INSERT INTO catalog.product_inventory (product_variant_id, available_quantity, reserved_quantity, sold_quantity)
SELECT variant.id, 100, 0, 0
FROM catalog.product_variants variant
JOIN catalog.products product ON product.id = variant.product_id
WHERE product.status = 'ACTIVE'
  AND variant.status = 'ACTIVE'
ON CONFLICT (product_variant_id) DO UPDATE
SET available_quantity = GREATEST(catalog.product_inventory.available_quantity, 100),
    reserved_quantity = 0,
    updated_at = CURRENT_TIMESTAMP;

-- ============================================================
-- Java backend dummy data for pgAdmin import
-- Scope: catalog, design, ordering only.
-- Try-on is intentionally skipped because it is postponed.
--
-- Suggested test header UUIDs:
--   Customer Minh: 22222222-2222-2222-2222-222222222222
--   Customer Linh: 33333333-3333-3333-3333-333333333333
--   Staff/Admin:   44444444-4444-4444-4444-444444444444
-- ============================================================

BEGIN;

-- ------------------------------------------------------------
-- Catalog: products
-- ------------------------------------------------------------
INSERT INTO catalog.products (
  id, name, description, base_price, status, created_by, created_at, updated_at
)
VALUES
  (
    '10000000-0000-0000-0000-000000000001',
    'Classic Cotton T-Shirt',
    'Regular-fit cotton tee for daily custom prints.',
    199000.00,
    'ACTIVE',
    '11111111-1111-1111-1111-111111111111',
    '2026-07-01T08:00:00+07:00',
    '2026-07-01T08:00:00+07:00'
  ),
  (
    '10000000-0000-0000-0000-000000000002',
    'Oversize Street T-Shirt',
    'Heavy oversize tee for large front/back graphics.',
    249000.00,
    'ACTIVE',
    '11111111-1111-1111-1111-111111111111',
    '2026-07-01T08:10:00+07:00',
    '2026-07-01T08:10:00+07:00'
  ),
  (
    '10000000-0000-0000-0000-000000000003',
    'Premium Soft T-Shirt',
    'Soft-touch premium tee for clean logo and typography designs.',
    299000.00,
    'ACTIVE',
    '11111111-1111-1111-1111-111111111111',
    '2026-07-01T08:20:00+07:00',
    '2026-07-01T08:20:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  base_price = EXCLUDED.base_price,
  status = EXCLUDED.status,
  created_by = EXCLUDED.created_by,
  updated_at = EXCLUDED.updated_at;

-- ------------------------------------------------------------
-- Catalog: variants
-- ------------------------------------------------------------
INSERT INTO catalog.product_variants (
  id, product_id, sku, size, color, material, price_adjustment, status, created_at, updated_at
)
VALUES
  (
    '20000000-0000-0000-0000-000000000001',
    '10000000-0000-0000-0000-000000000001',
    'CLASSIC-WHT-M',
    'M',
    'White',
    'Cotton 180gsm',
    0.00,
    'ACTIVE',
    '2026-07-01T08:30:00+07:00',
    '2026-07-01T08:30:00+07:00'
  ),
  (
    '20000000-0000-0000-0000-000000000002',
    '10000000-0000-0000-0000-000000000001',
    'CLASSIC-BLK-L',
    'L',
    'Black',
    'Cotton 180gsm',
    0.00,
    'ACTIVE',
    '2026-07-01T08:31:00+07:00',
    '2026-07-01T08:31:00+07:00'
  ),
  (
    '20000000-0000-0000-0000-000000000003',
    '10000000-0000-0000-0000-000000000002',
    'OVERSIZE-BLK-L',
    'L',
    'Black',
    'Heavy Cotton 240gsm',
    30000.00,
    'ACTIVE',
    '2026-07-01T08:32:00+07:00',
    '2026-07-01T08:32:00+07:00'
  ),
  (
    '20000000-0000-0000-0000-000000000004',
    '10000000-0000-0000-0000-000000000002',
    'OVERSIZE-CRM-XL',
    'XL',
    'Cream',
    'Heavy Cotton 240gsm',
    30000.00,
    'ACTIVE',
    '2026-07-01T08:33:00+07:00',
    '2026-07-01T08:33:00+07:00'
  ),
  (
    '20000000-0000-0000-0000-000000000005',
    '10000000-0000-0000-0000-000000000003',
    'PREMIUM-NVY-M',
    'M',
    'Navy',
    'Modal Cotton Blend',
    50000.00,
    'ACTIVE',
    '2026-07-01T08:34:00+07:00',
    '2026-07-01T08:34:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  product_id = EXCLUDED.product_id,
  sku = EXCLUDED.sku,
  size = EXCLUDED.size,
  color = EXCLUDED.color,
  material = EXCLUDED.material,
  price_adjustment = EXCLUDED.price_adjustment,
  status = EXCLUDED.status,
  updated_at = EXCLUDED.updated_at;

-- ------------------------------------------------------------
-- Catalog: images
-- ------------------------------------------------------------
INSERT INTO catalog.product_images (
  id, product_id, image_url, is_thumbnail, sort_order, created_at
)
VALUES
  (
    '30000000-0000-0000-0000-000000000001',
    '10000000-0000-0000-0000-000000000001',
    'http://localhost:19000/products/classic-cotton-front-white.png',
    TRUE,
    0,
    '2026-07-01T08:40:00+07:00'
  ),
  (
    '30000000-0000-0000-0000-000000000002',
    '10000000-0000-0000-0000-000000000001',
    'http://localhost:19000/products/classic-cotton-back-black.png',
    FALSE,
    1,
    '2026-07-01T08:41:00+07:00'
  ),
  (
    '30000000-0000-0000-0000-000000000003',
    '10000000-0000-0000-0000-000000000002',
    'http://localhost:19000/products/oversize-street-front-black.png',
    TRUE,
    0,
    '2026-07-01T08:42:00+07:00'
  ),
  (
    '30000000-0000-0000-0000-000000000004',
    '10000000-0000-0000-0000-000000000003',
    'http://localhost:19000/products/premium-soft-front-navy.png',
    TRUE,
    0,
    '2026-07-01T08:43:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  product_id = EXCLUDED.product_id,
  image_url = EXCLUDED.image_url,
  is_thumbnail = EXCLUDED.is_thumbnail,
  sort_order = EXCLUDED.sort_order;

-- ------------------------------------------------------------
-- Catalog: inventory
-- Pending order reserves 2 CLASSIC-WHT-M.
-- Paid/completed orders are reflected in sold_quantity.
-- ------------------------------------------------------------
INSERT INTO catalog.product_inventory (
  id, product_variant_id, available_quantity, reserved_quantity, sold_quantity, updated_at
)
VALUES
  (
    '40000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001',
    43,
    2,
    5,
    '2026-07-10T09:00:00+07:00'
  ),
  (
    '40000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000002',
    26,
    0,
    2,
    '2026-07-10T09:05:00+07:00'
  ),
  (
    '40000000-0000-0000-0000-000000000003',
    '20000000-0000-0000-0000-000000000003',
    18,
    0,
    1,
    '2026-07-10T09:10:00+07:00'
  ),
  (
    '40000000-0000-0000-0000-000000000004',
    '20000000-0000-0000-0000-000000000004',
    12,
    0,
    0,
    '2026-07-10T09:15:00+07:00'
  ),
  (
    '40000000-0000-0000-0000-000000000005',
    '20000000-0000-0000-0000-000000000005',
    9,
    0,
    1,
    '2026-07-10T09:20:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  product_variant_id = EXCLUDED.product_variant_id,
  available_quantity = EXCLUDED.available_quantity,
  reserved_quantity = EXCLUDED.reserved_quantity,
  sold_quantity = EXCLUDED.sold_quantity,
  updated_at = EXCLUDED.updated_at;

-- ------------------------------------------------------------
-- Designs
-- Canvas JSON convention used here:
--   base: product/variant view metadata for FE
--   printArea: target printable area on the shirt mockup
--   layers: visual layers matching design.design_layers rows
-- ------------------------------------------------------------
INSERT INTO design.designs (
  id, customer_id, product_id, product_variant_id, name, canvas_json,
  preview_image_url, print_file_url, status, created_at, updated_at
)
VALUES
  (
    '50000000-0000-0000-0000-000000000001',
    '22222222-2222-2222-2222-222222222222',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001',
    'Minimal Logo - Pocket Left',
    '{
      "version": 1,
      "base": {"productName": "Classic Cotton T-Shirt", "variantSku": "CLASSIC-WHT-M", "shirtColor": "White", "view": "front"},
      "printArea": {"x": 160, "y": 120, "width": 420, "height": 520, "unit": "px"},
      "layers": [
        {"id": "layer-logo-mark", "type": "ICON", "content": "spark-star", "x": 245, "y": 180, "width": 56, "height": 56, "rotation": 0, "color": "#111827", "zIndex": 1},
        {"id": "layer-logo-text", "type": "TEXT", "content": "AIFS", "x": 235, "y": 245, "width": 78, "height": 28, "rotation": 0, "color": "#111827", "font": "Inter SemiBold", "zIndex": 2}
      ]
    }'::jsonb,
    'http://localhost:19000/designs/minimal-logo-pocket-left-preview.png',
    'http://localhost:19000/prints/minimal-logo-pocket-left-print.pdf',
    'SAVED',
    '2026-07-11T10:00:00+07:00',
    '2026-07-11T10:20:00+07:00'
  ),
  (
    '50000000-0000-0000-0000-000000000002',
    '22222222-2222-2222-2222-222222222222',
    '10000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000003',
    'Street Back Graphic - Night Run',
    '{
      "version": 1,
      "base": {"productName": "Oversize Street T-Shirt", "variantSku": "OVERSIZE-BLK-L", "shirtColor": "Black", "view": "back"},
      "printArea": {"x": 120, "y": 90, "width": 520, "height": 650, "unit": "px"},
      "layers": [
        {"id": "layer-back-title", "type": "TEXT", "content": "NIGHT RUN", "x": 205, "y": 145, "width": 330, "height": 64, "rotation": 0, "color": "#F9FAFB", "font": "Bebas Neue", "zIndex": 1},
        {"id": "layer-back-graphic", "type": "IMAGE", "content": "http://localhost:19000/designs/assets/neon-runner-frame.png", "x": 175, "y": 235, "width": 390, "height": 300, "rotation": 0, "zIndex": 2},
        {"id": "layer-back-caption", "type": "TEXT", "content": "CUSTOM STUDIO 2026", "x": 235, "y": 565, "width": 260, "height": 34, "rotation": 0, "color": "#22D3EE", "font": "Inter Medium", "zIndex": 3}
      ]
    }'::jsonb,
    'http://localhost:19000/designs/street-back-night-run-preview.png',
    'http://localhost:19000/prints/street-back-night-run-print.pdf',
    'LOCKED',
    '2026-07-11T11:00:00+07:00',
    '2026-07-12T09:00:00+07:00'
  ),
  (
    '50000000-0000-0000-0000-000000000003',
    '33333333-3333-3333-3333-333333333333',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000002',
    'Retro Typography - Saigon Weekend',
    '{
      "version": 1,
      "base": {"productName": "Classic Cotton T-Shirt", "variantSku": "CLASSIC-BLK-L", "shirtColor": "Black", "view": "front"},
      "printArea": {"x": 150, "y": 115, "width": 440, "height": 540, "unit": "px"},
      "layers": [
        {"id": "layer-retro-main", "type": "TEXT", "content": "SAIGON", "x": 205, "y": 235, "width": 315, "height": 88, "rotation": -4, "color": "#F97316", "font": "Cooper Black", "zIndex": 1},
        {"id": "layer-retro-sub", "type": "TEXT", "content": "WEEKEND CLUB", "x": 235, "y": 330, "width": 245, "height": 36, "rotation": -4, "color": "#FDE68A", "font": "Inter Bold", "zIndex": 2}
      ]
    }'::jsonb,
    'http://localhost:19000/designs/retro-saigon-weekend-preview.png',
    'http://localhost:19000/prints/retro-saigon-weekend-print.pdf',
    'SAVED',
    '2026-07-12T13:30:00+07:00',
    '2026-07-12T14:00:00+07:00'
  ),
  (
    '50000000-0000-0000-0000-000000000004',
    '33333333-3333-3333-3333-333333333333',
    '10000000-0000-0000-0000-000000000003',
    '20000000-0000-0000-0000-000000000005',
    'Personal Monogram - LINH',
    '{
      "version": 1,
      "base": {"productName": "Premium Soft T-Shirt", "variantSku": "PREMIUM-NVY-M", "shirtColor": "Navy", "view": "front"},
      "printArea": {"x": 160, "y": 120, "width": 420, "height": 520, "unit": "px"},
      "layers": [
        {"id": "layer-monogram", "type": "TEXT", "content": "L", "x": 320, "y": 175, "width": 96, "height": 118, "rotation": 0, "color": "#E5E7EB", "font": "Playfair Display Bold", "zIndex": 1},
        {"id": "layer-name", "type": "TEXT", "content": "LINH PHAM", "x": 250, "y": 325, "width": 245, "height": 38, "rotation": 0, "color": "#93C5FD", "font": "Inter Medium", "zIndex": 2}
      ]
    }'::jsonb,
    'http://localhost:19000/designs/personal-monogram-linh-preview.png',
    'http://localhost:19000/prints/personal-monogram-linh-print.pdf',
    'LOCKED',
    '2026-07-12T15:00:00+07:00',
    '2026-07-13T08:45:00+07:00'
  ),
  (
    '50000000-0000-0000-0000-000000000005',
    '22222222-2222-2222-2222-222222222222',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001',
    'Summer Icon Pattern',
    '{
      "version": 1,
      "base": {"productName": "Classic Cotton T-Shirt", "variantSku": "CLASSIC-WHT-M", "shirtColor": "White", "view": "front"},
      "printArea": {"x": 160, "y": 120, "width": 420, "height": 520, "unit": "px"},
      "layers": [
        {"id": "layer-sun", "type": "ICON", "content": "sun", "x": 230, "y": 185, "width": 52, "height": 52, "rotation": 0, "color": "#FACC15", "zIndex": 1},
        {"id": "layer-wave", "type": "ICON", "content": "waves", "x": 310, "y": 245, "width": 72, "height": 48, "rotation": 0, "color": "#38BDF8", "zIndex": 2},
        {"id": "layer-copy", "type": "TEXT", "content": "GOOD DAYS", "x": 250, "y": 325, "width": 220, "height": 44, "rotation": 0, "color": "#0F172A", "font": "Inter ExtraBold", "zIndex": 3}
      ]
    }'::jsonb,
    'http://localhost:19000/designs/summer-icon-pattern-preview.png',
    'http://localhost:19000/prints/summer-icon-pattern-print.pdf',
    'SAVED',
    '2026-07-13T09:00:00+07:00',
    '2026-07-13T09:30:00+07:00'
  ),
  (
    '50000000-0000-0000-0000-000000000006',
    '22222222-2222-2222-2222-222222222222',
    '10000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000004',
    'Draft Oversize Cream Layout',
    '{"version": 1, "base": {"productName": "Oversize Street T-Shirt", "variantSku": "OVERSIZE-CRM-XL", "shirtColor": "Cream", "view": "front"}, "printArea": {"x": 120, "y": 90, "width": 520, "height": 650, "unit": "px"}, "layers": []}'::jsonb,
    NULL,
    NULL,
    'DRAFT',
    '2026-07-13T10:00:00+07:00',
    '2026-07-13T10:00:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  customer_id = EXCLUDED.customer_id,
  product_id = EXCLUDED.product_id,
  product_variant_id = EXCLUDED.product_variant_id,
  name = EXCLUDED.name,
  canvas_json = EXCLUDED.canvas_json,
  preview_image_url = EXCLUDED.preview_image_url,
  print_file_url = EXCLUDED.print_file_url,
  status = EXCLUDED.status,
  updated_at = EXCLUDED.updated_at;

-- ------------------------------------------------------------
-- Design layers
-- ------------------------------------------------------------
INSERT INTO design.design_layers (
  id, design_id, layer_type, content, position_x, position_y, width, height, rotation, color, z_index
)
VALUES
  ('51000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 'ICON', 'spark-star', 245.00, 180.00, 56.00, 56.00, 0.00, '#111827', 1),
  ('51000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000001', 'TEXT', 'AIFS', 235.00, 245.00, 78.00, 28.00, 0.00, '#111827', 2),
  ('51000000-0000-0000-0000-000000000003', '50000000-0000-0000-0000-000000000002', 'TEXT', 'NIGHT RUN', 205.00, 145.00, 330.00, 64.00, 0.00, '#F9FAFB', 1),
  ('51000000-0000-0000-0000-000000000004', '50000000-0000-0000-0000-000000000002', 'IMAGE', 'http://localhost:19000/designs/assets/neon-runner-frame.png', 175.00, 235.00, 390.00, 300.00, 0.00, NULL, 2),
  ('51000000-0000-0000-0000-000000000005', '50000000-0000-0000-0000-000000000002', 'TEXT', 'CUSTOM STUDIO 2026', 235.00, 565.00, 260.00, 34.00, 0.00, '#22D3EE', 3),
  ('51000000-0000-0000-0000-000000000006', '50000000-0000-0000-0000-000000000003', 'TEXT', 'SAIGON', 205.00, 235.00, 315.00, 88.00, -4.00, '#F97316', 1),
  ('51000000-0000-0000-0000-000000000007', '50000000-0000-0000-0000-000000000003', 'TEXT', 'WEEKEND CLUB', 235.00, 330.00, 245.00, 36.00, -4.00, '#FDE68A', 2),
  ('51000000-0000-0000-0000-000000000008', '50000000-0000-0000-0000-000000000004', 'TEXT', 'L', 320.00, 175.00, 96.00, 118.00, 0.00, '#E5E7EB', 1),
  ('51000000-0000-0000-0000-000000000009', '50000000-0000-0000-0000-000000000004', 'TEXT', 'LINH PHAM', 250.00, 325.00, 245.00, 38.00, 0.00, '#93C5FD', 2),
  ('51000000-0000-0000-0000-000000000010', '50000000-0000-0000-0000-000000000005', 'ICON', 'sun', 230.00, 185.00, 52.00, 52.00, 0.00, '#FACC15', 1),
  ('51000000-0000-0000-0000-000000000011', '50000000-0000-0000-0000-000000000005', 'ICON', 'waves', 310.00, 245.00, 72.00, 48.00, 0.00, '#38BDF8', 2),
  ('51000000-0000-0000-0000-000000000012', '50000000-0000-0000-0000-000000000005', 'TEXT', 'GOOD DAYS', 250.00, 325.00, 220.00, 44.00, 0.00, '#0F172A', 3)
ON CONFLICT (id) DO UPDATE SET
  design_id = EXCLUDED.design_id,
  layer_type = EXCLUDED.layer_type,
  content = EXCLUDED.content,
  position_x = EXCLUDED.position_x,
  position_y = EXCLUDED.position_y,
  width = EXCLUDED.width,
  height = EXCLUDED.height,
  rotation = EXCLUDED.rotation,
  color = EXCLUDED.color,
  z_index = EXCLUDED.z_index;

-- ------------------------------------------------------------
-- Cart
-- Cart items are not reserved yet. Checkout will create an order
-- and reserve inventory through Java OrderApplicationService.
-- ------------------------------------------------------------
INSERT INTO ordering.carts (
  id, customer_id, created_at, updated_at
)
VALUES
  (
    '70000000-0000-0000-0000-000000000001',
    '22222222-2222-2222-2222-222222222222',
    '2026-07-16T09:00:00+07:00',
    '2026-07-16T09:10:00+07:00'
  ),
  (
    '70000000-0000-0000-0000-000000000002',
    '33333333-3333-3333-3333-333333333333',
    '2026-07-16T09:20:00+07:00',
    '2026-07-16T09:25:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  customer_id = EXCLUDED.customer_id,
  updated_at = EXCLUDED.updated_at;

INSERT INTO ordering.cart_items (
  id, cart_id, product_id, product_variant_id, design_id, quantity, created_at, updated_at
)
VALUES
  (
    '71000000-0000-0000-0000-000000000001',
    '70000000-0000-0000-0000-000000000001',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001',
    '50000000-0000-0000-0000-000000000001',
    1,
    '2026-07-16T09:05:00+07:00',
    '2026-07-16T09:05:00+07:00'
  ),
  (
    '71000000-0000-0000-0000-000000000002',
    '70000000-0000-0000-0000-000000000002',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000002',
    '50000000-0000-0000-0000-000000000003',
    1,
    '2026-07-16T09:25:00+07:00',
    '2026-07-16T09:25:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  cart_id = EXCLUDED.cart_id,
  product_id = EXCLUDED.product_id,
  product_variant_id = EXCLUDED.product_variant_id,
  design_id = EXCLUDED.design_id,
  quantity = EXCLUDED.quantity,
  updated_at = EXCLUDED.updated_at;

-- ------------------------------------------------------------
-- Orders
-- ------------------------------------------------------------
INSERT INTO ordering.orders (
  id, order_code, customer_id, total_amount, payment_status, order_status,
  receiver_name, receiver_phone, shipping_address, created_at, updated_at
)
VALUES
  (
    '60000000-0000-0000-0000-000000000001',
    'ORD-DUMMY-0001',
    '22222222-2222-2222-2222-222222222222',
    398000.00,
    'PENDING',
    'PENDING_PAYMENT',
    'Minh Nguyen',
    '0901000001',
    '12 Nguyen Hue, District 1, Ho Chi Minh City',
    '2026-07-14T09:00:00+07:00',
    '2026-07-14T09:00:00+07:00'
  ),
  (
    '60000000-0000-0000-0000-000000000002',
    'ORD-DUMMY-0002',
    '22222222-2222-2222-2222-222222222222',
    279000.00,
    'PAID',
    'PAID',
    'Minh Nguyen',
    '0901000001',
    '12 Nguyen Hue, District 1, Ho Chi Minh City',
    '2026-07-14T10:00:00+07:00',
    '2026-07-14T10:30:00+07:00'
  ),
  (
    '60000000-0000-0000-0000-000000000003',
    'ORD-DUMMY-0003',
    '33333333-3333-3333-3333-333333333333',
    349000.00,
    'PAID',
    'IN_PRODUCTION',
    'Linh Pham',
    '0902000002',
    '88 Vo Van Tan, District 3, Ho Chi Minh City',
    '2026-07-15T08:00:00+07:00',
    '2026-07-15T13:00:00+07:00'
  ),
  (
    '60000000-0000-0000-0000-000000000004',
    'ORD-DUMMY-0004',
    '33333333-3333-3333-3333-333333333333',
    398000.00,
    'PAID',
    'COMPLETED',
    'Linh Pham',
    '0902000002',
    '88 Vo Van Tan, District 3, Ho Chi Minh City',
    '2026-07-10T08:00:00+07:00',
    '2026-07-12T18:00:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  order_code = EXCLUDED.order_code,
  customer_id = EXCLUDED.customer_id,
  total_amount = EXCLUDED.total_amount,
  payment_status = EXCLUDED.payment_status,
  order_status = EXCLUDED.order_status,
  receiver_name = EXCLUDED.receiver_name,
  receiver_phone = EXCLUDED.receiver_phone,
  shipping_address = EXCLUDED.shipping_address,
  updated_at = EXCLUDED.updated_at;

-- ------------------------------------------------------------
-- Order items
-- ------------------------------------------------------------
INSERT INTO ordering.order_items (
  id, order_id, product_id, product_variant_id, design_id,
  product_name_snapshot, variant_snapshot, quantity, unit_price, total_price
)
VALUES
  (
    '61000000-0000-0000-0000-000000000001',
    '60000000-0000-0000-0000-000000000001',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001',
    '50000000-0000-0000-0000-000000000005',
    'Classic Cotton T-Shirt',
    '{"sku": "CLASSIC-WHT-M", "size": "M", "color": "White", "material": "Cotton 180gsm"}'::jsonb,
    2,
    199000.00,
    398000.00
  ),
  (
    '61000000-0000-0000-0000-000000000002',
    '60000000-0000-0000-0000-000000000002',
    '10000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000003',
    '50000000-0000-0000-0000-000000000002',
    'Oversize Street T-Shirt',
    '{"sku": "OVERSIZE-BLK-L", "size": "L", "color": "Black", "material": "Heavy Cotton 240gsm"}'::jsonb,
    1,
    279000.00,
    279000.00
  ),
  (
    '61000000-0000-0000-0000-000000000003',
    '60000000-0000-0000-0000-000000000003',
    '10000000-0000-0000-0000-000000000003',
    '20000000-0000-0000-0000-000000000005',
    '50000000-0000-0000-0000-000000000004',
    'Premium Soft T-Shirt',
    '{"sku": "PREMIUM-NVY-M", "size": "M", "color": "Navy", "material": "Modal Cotton Blend"}'::jsonb,
    1,
    349000.00,
    349000.00
  ),
  (
    '61000000-0000-0000-0000-000000000004',
    '60000000-0000-0000-0000-000000000004',
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000002',
    '50000000-0000-0000-0000-000000000003',
    'Classic Cotton T-Shirt',
    '{"sku": "CLASSIC-BLK-L", "size": "L", "color": "Black", "material": "Cotton 180gsm"}'::jsonb,
    2,
    199000.00,
    398000.00
  )
ON CONFLICT (id) DO UPDATE SET
  order_id = EXCLUDED.order_id,
  product_id = EXCLUDED.product_id,
  product_variant_id = EXCLUDED.product_variant_id,
  design_id = EXCLUDED.design_id,
  product_name_snapshot = EXCLUDED.product_name_snapshot,
  variant_snapshot = EXCLUDED.variant_snapshot,
  quantity = EXCLUDED.quantity,
  unit_price = EXCLUDED.unit_price,
  total_price = EXCLUDED.total_price;

-- ------------------------------------------------------------
-- Order status history
-- ------------------------------------------------------------
INSERT INTO ordering.order_status_history (
  id, order_id, from_status, to_status, changed_by, note, created_at
)
VALUES
  (
    '62000000-0000-0000-0000-000000000001',
    '60000000-0000-0000-0000-000000000001',
    NULL,
    'PENDING_PAYMENT',
    '22222222-2222-2222-2222-222222222222',
    'Order created, waiting for payment',
    '2026-07-14T09:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000002',
    '60000000-0000-0000-0000-000000000002',
    NULL,
    'PENDING_PAYMENT',
    '22222222-2222-2222-2222-222222222222',
    'Order created',
    '2026-07-14T10:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000003',
    '60000000-0000-0000-0000-000000000002',
    'PENDING_PAYMENT',
    'PAID',
    '22222222-2222-2222-2222-222222222222',
    'Payment succeeded: TXN-DUMMY-0002',
    '2026-07-14T10:30:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000004',
    '60000000-0000-0000-0000-000000000003',
    NULL,
    'PENDING_PAYMENT',
    '33333333-3333-3333-3333-333333333333',
    'Order created',
    '2026-07-15T08:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000005',
    '60000000-0000-0000-0000-000000000003',
    'PENDING_PAYMENT',
    'PAID',
    '33333333-3333-3333-3333-333333333333',
    'Payment succeeded: TXN-DUMMY-0003',
    '2026-07-15T08:30:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000006',
    '60000000-0000-0000-0000-000000000003',
    'PAID',
    'IN_PRODUCTION',
    '44444444-4444-4444-4444-444444444444',
    'Staff started printing',
    '2026-07-15T13:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000007',
    '60000000-0000-0000-0000-000000000004',
    NULL,
    'PENDING_PAYMENT',
    '33333333-3333-3333-3333-333333333333',
    'Order created',
    '2026-07-10T08:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000008',
    '60000000-0000-0000-0000-000000000004',
    'PENDING_PAYMENT',
    'PAID',
    '33333333-3333-3333-3333-333333333333',
    'Payment succeeded: TXN-DUMMY-0004',
    '2026-07-10T08:25:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000009',
    '60000000-0000-0000-0000-000000000004',
    'PAID',
    'IN_PRODUCTION',
    '44444444-4444-4444-4444-444444444444',
    'Printing started',
    '2026-07-10T13:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000010',
    '60000000-0000-0000-0000-000000000004',
    'IN_PRODUCTION',
    'SHIPPING',
    '44444444-4444-4444-4444-444444444444',
    'Handed over to shipper',
    '2026-07-11T11:00:00+07:00'
  ),
  (
    '62000000-0000-0000-0000-000000000011',
    '60000000-0000-0000-0000-000000000004',
    'SHIPPING',
    'COMPLETED',
    '44444444-4444-4444-4444-444444444444',
    'Delivered successfully',
    '2026-07-12T18:00:00+07:00'
  )
ON CONFLICT (id) DO UPDATE SET
  order_id = EXCLUDED.order_id,
  from_status = EXCLUDED.from_status,
  to_status = EXCLUDED.to_status,
  changed_by = EXCLUDED.changed_by,
  note = EXCLUDED.note,
  created_at = EXCLUDED.created_at;

COMMIT;

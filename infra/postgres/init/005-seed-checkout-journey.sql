-- ============================================================
-- Demo checkout journey seed
-- Covers: customer -> product -> design/try-on -> order -> payment -> invoice.
-- Idempotent: safe to run multiple times.
-- ============================================================

-- Shared demo IDs
-- Customer email/password for FE auth: checkout.demo@aifashion.com / Password123!

INSERT INTO identity.roles (id, code, name, description)
VALUES
  ('71000000-0000-4000-8000-000000000003'::uuid, 'CUSTOMER', 'Khach hang', 'Customer shopping and customization role')
ON CONFLICT (code) DO NOTHING;

INSERT INTO identity.users (id, email, password_hash, full_name, phone, avatar_url, status)
VALUES (
  '77777777-7777-4777-8777-777777777777'::uuid,
  'checkout.demo@aifashion.com',
  '100000.AQIDBAUGBwgJCgsMDQ4PEA==.asTsfMfYM0AG5BVkQcjt6dH2Gw8L84DpMiBV83B0m3Q=',
  'Checkout Demo Customer',
  '0900999001',
  'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=512&q=80',
  'ACTIVE'
)
ON CONFLICT (email) DO UPDATE
SET full_name = EXCLUDED.full_name,
    phone = EXCLUDED.phone,
    avatar_url = EXCLUDED.avatar_url,
    status = EXCLUDED.status,
    updated_at = CURRENT_TIMESTAMP;

INSERT INTO identity.user_roles (id, user_id, role_id)
SELECT
  '72000000-0000-4000-8000-000000000003'::uuid,
  '77777777-7777-4777-8777-777777777777'::uuid,
  role.id
FROM identity.roles role
WHERE role.code = 'CUSTOMER'
ON CONFLICT (user_id, role_id) DO NOTHING;

-- Keep demo product inventory healthy for checkout scenarios.
UPDATE catalog.product_inventory
SET available_quantity = GREATEST(available_quantity, 40),
    reserved_quantity = 0,
    updated_at = CURRENT_TIMESTAMP
WHERE product_variant_id IN (
  '11111111-b112-4111-8111-111111111111'::uuid,
  '22222222-b111-4222-8222-222222222222'::uuid,
  '33333333-b111-4333-8333-333333333333'::uuid
);

-- Designs representing the customer journey.
WITH demo_designs (
  id, customer_id, product_id, product_variant_id, name, canvas_json,
  preview_image_url, print_file_url, status
) AS (
  VALUES
    (
      '73000000-0000-4000-8000-000000000001'::uuid,
      '77777777-7777-4777-8777-777777777777'::uuid,
      '11111111-1111-4111-8111-111111111111'::uuid,
      '11111111-b112-4111-8111-111111111111'::uuid,
      'Saved classic tee design',
      '{"canvas":{"width":1024,"height":1024},"layers":[{"type":"TEXT","content":"AI FASHION","x":310,"y":420,"color":"#111111"}]}'::jsonb,
      'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=1200&q=80',
      'http://localhost:19000/prints/demo/classic-tee-print.pdf',
      'SAVED'
    ),
    (
      '73000000-0000-4000-8000-000000000002'::uuid,
      '77777777-7777-4777-8777-777777777777'::uuid,
      '22222222-2222-4222-8222-222222222222'::uuid,
      '22222222-b111-4222-8222-222222222222'::uuid,
      'Pending payment streetwear design',
      '{"canvas":{"width":1024,"height":1024},"layers":[{"type":"TEXT","content":"STUDIO DROP","x":280,"y":390,"color":"#f5f5f5"}]}'::jsonb,
      'https://images.unsplash.com/photo-1576566588028-4147f3842f27?auto=format&fit=crop&w=1200&q=80',
      'http://localhost:19000/prints/demo/streetwear-print.pdf',
      'LOCKED'
    ),
    (
      '73000000-0000-4000-8000-000000000003'::uuid,
      '77777777-7777-4777-8777-777777777777'::uuid,
      '33333333-3333-4333-8333-333333333333'::uuid,
      '33333333-b111-4333-8333-333333333333'::uuid,
      'Paid hoodie design',
      '{"canvas":{"width":1024,"height":1024},"layers":[{"type":"TEXT","content":"NEURAL HOODIE","x":245,"y":360,"color":"#ffffff"}]}'::jsonb,
      'https://images.unsplash.com/photo-1556821840-3a63f95609a7?auto=format&fit=crop&w=1200&q=80',
      'http://localhost:19000/prints/demo/hoodie-print.pdf',
      'LOCKED'
    )
)
INSERT INTO design.designs (
  id, customer_id, product_id, product_variant_id, name, canvas_json,
  preview_image_url, print_file_url, status
)
SELECT id, customer_id, product_id, product_variant_id, name, canvas_json, preview_image_url, print_file_url, status
FROM demo_designs
ON CONFLICT (id) DO UPDATE
SET name = EXCLUDED.name,
    canvas_json = EXCLUDED.canvas_json,
    preview_image_url = EXCLUDED.preview_image_url,
    print_file_url = EXCLUDED.print_file_url,
    status = EXCLUDED.status,
    updated_at = CURRENT_TIMESTAMP;

WITH demo_layers (id, design_id, layer_type, content, position_x, position_y, width, height, rotation, color, z_index) AS (
  VALUES
    ('73100000-0000-4000-8000-000000000001'::uuid, '73000000-0000-4000-8000-000000000001'::uuid, 'TEXT', 'AI FASHION', 310.00, 420.00, 420.00, 96.00, 0.00, '#111111', 1),
    ('73100000-0000-4000-8000-000000000002'::uuid, '73000000-0000-4000-8000-000000000002'::uuid, 'TEXT', 'STUDIO DROP', 280.00, 390.00, 480.00, 100.00, 0.00, '#f5f5f5', 1),
    ('73100000-0000-4000-8000-000000000003'::uuid, '73000000-0000-4000-8000-000000000003'::uuid, 'TEXT', 'NEURAL HOODIE', 245.00, 360.00, 560.00, 110.00, 0.00, '#ffffff', 1)
)
INSERT INTO design.design_layers (
  id, design_id, layer_type, content, position_x, position_y, width, height, rotation, color, z_index
)
SELECT id, design_id, layer_type, content, position_x, position_y, width, height, rotation, color, z_index
FROM demo_layers
ON CONFLICT (id) DO UPDATE
SET content = EXCLUDED.content,
    position_x = EXCLUDED.position_x,
    position_y = EXCLUDED.position_y,
    width = EXCLUDED.width,
    height = EXCLUDED.height,
    rotation = EXCLUDED.rotation,
    color = EXCLUDED.color,
    z_index = EXCLUDED.z_index;

-- Optional try-on success data for the paid hoodie design.
INSERT INTO ai_tryon.tryon_requests (
  id, customer_id, design_id, user_photo_url, height_cm, weight_kg, status, requested_at, completed_at
)
VALUES (
  '74000000-0000-4000-8000-000000000001'::uuid,
  '77777777-7777-4777-8777-777777777777'::uuid,
  '73000000-0000-4000-8000-000000000003'::uuid,
  'http://localhost:19000/tryon/demo/customer-photo.jpg',
  172.00,
  64.00,
  'SUCCEEDED',
  CURRENT_TIMESTAMP - INTERVAL '3 days',
  CURRENT_TIMESTAMP - INTERVAL '3 days' + INTERVAL '2 minutes'
)
ON CONFLICT (id) DO UPDATE
SET status = EXCLUDED.status,
    completed_at = EXCLUDED.completed_at,
    error_message = NULL;

INSERT INTO ai_tryon.tryon_results (
  id, tryon_request_id, design_id, result_image_url, processing_time_ms
)
VALUES (
  '74100000-0000-4000-8000-000000000001'::uuid,
  '74000000-0000-4000-8000-000000000001'::uuid,
  '73000000-0000-4000-8000-000000000003'::uuid,
  'http://localhost:19000/tryon/demo/hoodie-result.jpg',
  118000
)
ON CONFLICT (tryon_request_id) DO UPDATE
SET result_image_url = EXCLUDED.result_image_url,
    processing_time_ms = EXCLUDED.processing_time_ms;

-- Orders: one pending payment, one paid/in production, one completed.
WITH demo_orders (
  id, order_code, customer_id, total_amount, payment_status, order_status,
  receiver_name, receiver_phone, shipping_address, created_at
) AS (
  VALUES
    (
      '75000000-0000-4000-8000-000000000001'::uuid,
      'DEMO-ORDER-PENDING',
      '77777777-7777-4777-8777-777777777777'::uuid,
      518000.00,
      'PENDING',
      'PENDING_PAYMENT',
      'Checkout Demo Customer',
      '0900999001',
      '123 Nguyen Hue, Quan 1, TP. Ho Chi Minh',
      CURRENT_TIMESTAMP - INTERVAL '1 day'
    ),
    (
      '75000000-0000-4000-8000-000000000002'::uuid,
      'DEMO-ORDER-PAID',
      '77777777-7777-4777-8777-777777777777'::uuid,
      519000.00,
      'PAID',
      'IN_PRODUCTION',
      'Checkout Demo Customer',
      '0900999001',
      '123 Nguyen Hue, Quan 1, TP. Ho Chi Minh',
      CURRENT_TIMESTAMP - INTERVAL '3 days'
    ),
    (
      '75000000-0000-4000-8000-000000000003'::uuid,
      'DEMO-ORDER-COMPLETED',
      '77777777-7777-4777-8777-777777777777'::uuid,
      519000.00,
      'PAID',
      'COMPLETED',
      'Checkout Demo Customer',
      '0900999001',
      '123 Nguyen Hue, Quan 1, TP. Ho Chi Minh',
      CURRENT_TIMESTAMP - INTERVAL '7 days'
    )
)
INSERT INTO ordering.orders (
  id, order_code, customer_id, total_amount, payment_status, order_status,
  receiver_name, receiver_phone, shipping_address, created_at, updated_at
)
SELECT id, order_code, customer_id, total_amount, payment_status, order_status,
       receiver_name, receiver_phone, shipping_address, created_at, CURRENT_TIMESTAMP
FROM demo_orders
ON CONFLICT (order_code) DO UPDATE
SET total_amount = EXCLUDED.total_amount,
    payment_status = EXCLUDED.payment_status,
    order_status = EXCLUDED.order_status,
    receiver_name = EXCLUDED.receiver_name,
    receiver_phone = EXCLUDED.receiver_phone,
    shipping_address = EXCLUDED.shipping_address,
    updated_at = CURRENT_TIMESTAMP;

WITH demo_items (
  id, order_id, product_id, product_variant_id, design_id,
  product_name_snapshot, variant_snapshot, quantity, unit_price, total_price
) AS (
  VALUES
    (
      '75100000-0000-4000-8000-000000000001'::uuid,
      '75000000-0000-4000-8000-000000000001'::uuid,
      '22222222-2222-4222-8222-222222222222'::uuid,
      '22222222-b111-4222-8222-222222222222'::uuid,
      '73000000-0000-4000-8000-000000000002'::uuid,
      'Oversized Streetwear Tee',
      '{"size":"M","color":"Black","material":"Heavy Cotton","sku":"AFS-TEE-OVERSIZE-BLK-M"}'::jsonb,
      2,
      259000.00,
      518000.00
    ),
    (
      '75100000-0000-4000-8000-000000000002'::uuid,
      '75000000-0000-4000-8000-000000000002'::uuid,
      '33333333-3333-4333-8333-333333333333'::uuid,
      '33333333-b111-4333-8333-333333333333'::uuid,
      '73000000-0000-4000-8000-000000000003'::uuid,
      'Premium Hoodie',
      '{"size":"M","color":"Black","material":"Fleece","sku":"AFS-HOODIE-PRM-BLK-M"}'::jsonb,
      1,
      519000.00,
      519000.00
    ),
    (
      '75100000-0000-4000-8000-000000000003'::uuid,
      '75000000-0000-4000-8000-000000000003'::uuid,
      '33333333-3333-4333-8333-333333333333'::uuid,
      '33333333-b111-4333-8333-333333333333'::uuid,
      '73000000-0000-4000-8000-000000000003'::uuid,
      'Premium Hoodie',
      '{"size":"M","color":"Black","material":"Fleece","sku":"AFS-HOODIE-PRM-BLK-M"}'::jsonb,
      1,
      519000.00,
      519000.00
    )
)
INSERT INTO ordering.order_items (
  id, order_id, product_id, product_variant_id, design_id,
  product_name_snapshot, variant_snapshot, quantity, unit_price, total_price
)
SELECT id, order_id, product_id, product_variant_id, design_id,
       product_name_snapshot, variant_snapshot, quantity, unit_price, total_price
FROM demo_items
ON CONFLICT (id) DO UPDATE
SET product_name_snapshot = EXCLUDED.product_name_snapshot,
    variant_snapshot = EXCLUDED.variant_snapshot,
    quantity = EXCLUDED.quantity,
    unit_price = EXCLUDED.unit_price,
    total_price = EXCLUDED.total_price;

WITH demo_history (id, order_id, from_status, to_status, changed_by, note, created_at) AS (
  VALUES
    ('75200000-0000-4000-8000-000000000001'::uuid, '75000000-0000-4000-8000-000000000001'::uuid, NULL, 'PENDING_PAYMENT', '77777777-7777-4777-8777-777777777777'::uuid, 'Order created and waiting for payment.', CURRENT_TIMESTAMP - INTERVAL '1 day'),
    ('75200000-0000-4000-8000-000000000002'::uuid, '75000000-0000-4000-8000-000000000002'::uuid, NULL, 'PENDING_PAYMENT', '77777777-7777-4777-8777-777777777777'::uuid, 'Order created.', CURRENT_TIMESTAMP - INTERVAL '3 days'),
    ('75200000-0000-4000-8000-000000000003'::uuid, '75000000-0000-4000-8000-000000000002'::uuid, 'PENDING_PAYMENT', 'PAID', '77777777-7777-4777-8777-777777777777'::uuid, 'Payment completed.', CURRENT_TIMESTAMP - INTERVAL '3 days' + INTERVAL '5 minutes'),
    ('75200000-0000-4000-8000-000000000004'::uuid, '75000000-0000-4000-8000-000000000002'::uuid, 'PAID', 'IN_PRODUCTION', '71000000-0000-4000-8000-000000000002'::uuid, 'Staff started production.', CURRENT_TIMESTAMP - INTERVAL '2 days'),
    ('75200000-0000-4000-8000-000000000005'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, NULL, 'PENDING_PAYMENT', '77777777-7777-4777-8777-777777777777'::uuid, 'Order created.', CURRENT_TIMESTAMP - INTERVAL '7 days'),
    ('75200000-0000-4000-8000-000000000006'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, 'PENDING_PAYMENT', 'PAID', '77777777-7777-4777-8777-777777777777'::uuid, 'Payment completed.', CURRENT_TIMESTAMP - INTERVAL '7 days' + INTERVAL '5 minutes'),
    ('75200000-0000-4000-8000-000000000007'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, 'PAID', 'IN_PRODUCTION', '71000000-0000-4000-8000-000000000002'::uuid, 'Production started.', CURRENT_TIMESTAMP - INTERVAL '6 days'),
    ('75200000-0000-4000-8000-000000000008'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, 'IN_PRODUCTION', 'SHIPPING', '71000000-0000-4000-8000-000000000002'::uuid, 'Handed over to shipping.', CURRENT_TIMESTAMP - INTERVAL '5 days'),
    ('75200000-0000-4000-8000-000000000009'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, 'SHIPPING', 'COMPLETED', '71000000-0000-4000-8000-000000000002'::uuid, 'Delivered successfully.', CURRENT_TIMESTAMP - INTERVAL '4 days')
)
INSERT INTO ordering.order_status_history (id, order_id, from_status, to_status, changed_by, note, created_at)
SELECT id, order_id, from_status, to_status, changed_by, note, created_at
FROM demo_history
ON CONFLICT (id) DO UPDATE
SET from_status = EXCLUDED.from_status,
    to_status = EXCLUDED.to_status,
    changed_by = EXCLUDED.changed_by,
    note = EXCLUDED.note,
    created_at = EXCLUDED.created_at;

-- Java-side payment table from the shared MVP schema.
WITH demo_payments (
  id, order_id, customer_id, amount, payment_method, payment_status,
  transaction_code, invoice_number, invoice_pdf_url, paid_at, created_at
) AS (
  VALUES
    ('76000000-0000-4000-8000-000000000001'::uuid, '75000000-0000-4000-8000-000000000001'::uuid, '77777777-7777-4777-8777-777777777777'::uuid, 518000.00, 'MOCK', 'PENDING', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP - INTERVAL '1 day'),
    ('76000000-0000-4000-8000-000000000002'::uuid, '75000000-0000-4000-8000-000000000002'::uuid, '77777777-7777-4777-8777-777777777777'::uuid, 519000.00, 'MOCK', 'PAID', 'DEMO-TXN-PAID-001', 'INV-DEMO-PAID-001', 'http://localhost:19000/invoices/demo/inv-demo-paid-001.pdf', CURRENT_TIMESTAMP - INTERVAL '3 days' + INTERVAL '5 minutes', CURRENT_TIMESTAMP - INTERVAL '3 days'),
    ('76000000-0000-4000-8000-000000000003'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, '77777777-7777-4777-8777-777777777777'::uuid, 519000.00, 'MOCK', 'PAID', 'DEMO-TXN-COMPLETE-001', 'INV-DEMO-COMPLETE-001', 'http://localhost:19000/invoices/demo/inv-demo-complete-001.pdf', CURRENT_TIMESTAMP - INTERVAL '7 days' + INTERVAL '5 minutes', CURRENT_TIMESTAMP - INTERVAL '7 days')
)
INSERT INTO payment.payments (
  id, order_id, customer_id, amount, payment_method, payment_status,
  transaction_code, invoice_number, invoice_pdf_url, paid_at, created_at
)
SELECT id, order_id, customer_id, amount, payment_method, payment_status,
       transaction_code, invoice_number, invoice_pdf_url, paid_at, created_at
FROM demo_payments
ON CONFLICT (id) DO UPDATE
SET amount = EXCLUDED.amount,
    payment_method = EXCLUDED.payment_method,
    payment_status = EXCLUDED.payment_status,
    transaction_code = EXCLUDED.transaction_code,
    invoice_number = EXCLUDED.invoice_number,
    invoice_pdf_url = EXCLUDED.invoice_pdf_url,
    paid_at = EXCLUDED.paid_at;

-- Dotnet public schema seed. Guarded because these tables are created by EF migrations,
-- not by the infra init SQL on first database boot.
DO $$
DECLARE
  customer_id uuid := '77777777-7777-4777-8777-777777777777'::uuid;
  customer_role_id uuid := '71000000-0000-4000-8000-000000000003'::uuid;
  actual_customer_role_id uuid;
BEGIN
  IF to_regclass('public.users') IS NOT NULL THEN
    INSERT INTO public.roles ("Id", code, name, description, created_at)
    VALUES (customer_role_id, 'CUSTOMER', 'Customer', 'Customer role', CURRENT_TIMESTAMP)
    ON CONFLICT (code) DO NOTHING;

    SELECT "Id"
    INTO actual_customer_role_id
    FROM public.roles
    WHERE code = 'CUSTOMER'
    LIMIT 1;

    INSERT INTO public.users (
      "Id", email, password_hash, full_name, phone, avatar_url, status, created_at, updated_at
    )
    VALUES (
      customer_id,
      'checkout.demo@aifashion.com',
      '100000.AQIDBAUGBwgJCgsMDQ4PEA==.asTsfMfYM0AG5BVkQcjt6dH2Gw8L84DpMiBV83B0m3Q=',
      'Checkout Demo Customer',
      '0900999001',
      'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=512&q=80',
      'ACTIVE',
      CURRENT_TIMESTAMP,
      CURRENT_TIMESTAMP
    )
    ON CONFLICT (email) DO UPDATE
    SET full_name = EXCLUDED.full_name,
        phone = EXCLUDED.phone,
        avatar_url = EXCLUDED.avatar_url,
        status = EXCLUDED.status,
        updated_at = CURRENT_TIMESTAMP;

    INSERT INTO public.user_roles ("Id", user_id, role_id, created_at)
    VALUES ('72000000-0000-4000-8000-000000000103'::uuid, customer_id, actual_customer_role_id, CURRENT_TIMESTAMP)
    ON CONFLICT (user_id, role_id) DO NOTHING;
  END IF;

  IF to_regclass('public.payment_orders') IS NOT NULL THEN
    INSERT INTO public.payment_orders (
      "Id", user_id, order_id, order_code, amount, description,
      payment_link_id, status, gateway_reference, paid_at, cancelled_at, created_at, updated_at
    )
    VALUES
      ('77000000-0000-4000-8000-000000000001'::uuid, customer_id, '75000000-0000-4000-8000-000000000001'::uuid, 90010001, 518000, 'Thanh toan demo order pending', 'DEMO-PAYLINK-PENDING-001', 'Pending', NULL, NULL, NULL, CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP),
      ('77000000-0000-4000-8000-000000000002'::uuid, customer_id, '75000000-0000-4000-8000-000000000002'::uuid, 90010002, 519000, 'Thanh toan demo order paid', 'DEMO-PAYLINK-PAID-001', 'Paid', 'DEMO-GW-PAID-001', CURRENT_TIMESTAMP - INTERVAL '3 days' + INTERVAL '5 minutes', NULL, CURRENT_TIMESTAMP - INTERVAL '3 days', CURRENT_TIMESTAMP),
      ('77000000-0000-4000-8000-000000000003'::uuid, customer_id, '75000000-0000-4000-8000-000000000003'::uuid, 90010003, 519000, 'Thanh toan demo order completed', 'DEMO-PAYLINK-COMPLETE-001', 'Paid', 'DEMO-GW-COMPLETE-001', CURRENT_TIMESTAMP - INTERVAL '7 days' + INTERVAL '5 minutes', NULL, CURRENT_TIMESTAMP - INTERVAL '7 days', CURRENT_TIMESTAMP)
    ON CONFLICT (order_code) DO UPDATE
    SET order_id = EXCLUDED.order_id,
        amount = EXCLUDED.amount,
        description = EXCLUDED.description,
        payment_link_id = EXCLUDED.payment_link_id,
        status = EXCLUDED.status,
        gateway_reference = EXCLUDED.gateway_reference,
        paid_at = EXCLUDED.paid_at,
        updated_at = CURRENT_TIMESTAMP;
  END IF;

  IF to_regclass('public.invoices') IS NOT NULL THEN
    INSERT INTO public.invoices (
      "Id", order_id, payment_id, customer_id, invoice_number, total_amount,
      currency, status, pdf_url, issued_at, created_at
    )
    VALUES
      ('78000000-0000-4000-8000-000000000002'::uuid, '75000000-0000-4000-8000-000000000002'::uuid, '77000000-0000-4000-8000-000000000002'::uuid, customer_id, 'INV-DEMO-PAID-001', 519000.00, 'VND', 'Paid', 'http://localhost:19000/invoices/demo/inv-demo-paid-001.pdf', CURRENT_TIMESTAMP - INTERVAL '3 days' + INTERVAL '5 minutes', CURRENT_TIMESTAMP - INTERVAL '3 days'),
      ('78000000-0000-4000-8000-000000000003'::uuid, '75000000-0000-4000-8000-000000000003'::uuid, '77000000-0000-4000-8000-000000000003'::uuid, customer_id, 'INV-DEMO-COMPLETE-001', 519000.00, 'VND', 'Paid', 'http://localhost:19000/invoices/demo/inv-demo-complete-001.pdf', CURRENT_TIMESTAMP - INTERVAL '7 days' + INTERVAL '5 minutes', CURRENT_TIMESTAMP - INTERVAL '7 days')
    ON CONFLICT (order_id) DO UPDATE
    SET payment_id = EXCLUDED.payment_id,
        total_amount = EXCLUDED.total_amount,
        status = EXCLUDED.status,
        pdf_url = EXCLUDED.pdf_url,
        issued_at = EXCLUDED.issued_at;

    INSERT INTO public.invoice_items (
      "Id", invoice_id, product_name_snapshot, variant_snapshot, quantity, unit_price, created_at
    )
    VALUES
      ('78100000-0000-4000-8000-000000000002'::uuid, '78000000-0000-4000-8000-000000000002'::uuid, 'Premium Hoodie', '{"size":"M","color":"Black","material":"Fleece","sku":"AFS-HOODIE-PRM-BLK-M"}', 1, 519000.00, CURRENT_TIMESTAMP - INTERVAL '3 days'),
      ('78100000-0000-4000-8000-000000000003'::uuid, '78000000-0000-4000-8000-000000000003'::uuid, 'Premium Hoodie', '{"size":"M","color":"Black","material":"Fleece","sku":"AFS-HOODIE-PRM-BLK-M"}', 1, 519000.00, CURRENT_TIMESTAMP - INTERVAL '7 days')
    ON CONFLICT ("Id") DO UPDATE
    SET product_name_snapshot = EXCLUDED.product_name_snapshot,
        variant_snapshot = EXCLUDED.variant_snapshot,
        quantity = EXCLUDED.quantity,
        unit_price = EXCLUDED.unit_price;
  END IF;
END $$;

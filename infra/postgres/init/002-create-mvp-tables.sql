-- ============================================================
-- MVP tables (18) — PostgreSQL
-- UUID via gen_random_uuid() (pgcrypto, enabled in 001)
-- All timestamps are TIMESTAMPTZ (stored as UTC)
-- This is the shared MVP database. Cross-service references stay as UUID values;
-- only the owning service should write to its own schema/table.
-- ============================================================

CREATE TABLE IF NOT EXISTS identity.users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  phone VARCHAR(20),
  avatar_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_users_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'BANNED'))
);

CREATE TABLE IF NOT EXISTS identity.roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code VARCHAR(50) NOT NULL UNIQUE,
  name VARCHAR(100) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS identity.user_roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES identity.users(id),
  role_id UUID NOT NULL REFERENCES identity.roles(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_user_roles_user_role UNIQUE (user_id, role_id)
);

CREATE TABLE IF NOT EXISTS identity.refresh_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES identity.users(id),
  token_hash TEXT NOT NULL,
  expires_at TIMESTAMPTZ NOT NULL,
  revoked_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS catalog.products (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(255) NOT NULL,
  description TEXT,
  base_price DECIMAL(18,2) NOT NULL,
  status VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
  created_by UUID,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_products_base_price CHECK (base_price >= 0),
  CONSTRAINT ck_products_status CHECK (status IN ('DRAFT', 'ACTIVE', 'INACTIVE', 'ARCHIVED'))
);

CREATE TABLE IF NOT EXISTS catalog.product_variants (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  sku VARCHAR(100) NOT NULL UNIQUE,
  size VARCHAR(50) NOT NULL,
  color VARCHAR(100) NOT NULL,
  material VARCHAR(100) NOT NULL,
  price_adjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
  status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_product_variants_price_adjustment CHECK (price_adjustment >= 0),
  CONSTRAINT ck_product_variants_status CHECK (status IN ('ACTIVE', 'INACTIVE'))
);

CREATE TABLE IF NOT EXISTS catalog.product_images (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  image_url TEXT NOT NULL,
  is_thumbnail BOOLEAN NOT NULL DEFAULT FALSE,
  sort_order INT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_product_images_sort_order CHECK (sort_order >= 0)
);

CREATE TABLE IF NOT EXISTS catalog.product_inventory (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  product_variant_id UUID NOT NULL UNIQUE REFERENCES catalog.product_variants(id),
  available_quantity INT NOT NULL DEFAULT 0,
  reserved_quantity INT NOT NULL DEFAULT 0,
  sold_quantity INT NOT NULL DEFAULT 0,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_inventory_quantities CHECK (
    available_quantity >= 0
    AND reserved_quantity >= 0
    AND sold_quantity >= 0
  )
);

CREATE TABLE IF NOT EXISTS design.designs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  customer_id UUID NOT NULL,
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  product_variant_id UUID NOT NULL REFERENCES catalog.product_variants(id),
  name VARCHAR(255) NOT NULL,
  canvas_json JSONB NOT NULL,
  preview_image_url TEXT,
  print_file_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_designs_status CHECK (status IN ('DRAFT', 'SAVED', 'LOCKED'))
);

CREATE TABLE IF NOT EXISTS design.design_layers (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  layer_type VARCHAR(50) NOT NULL,
  content TEXT,
  position_x DECIMAL(10,2) NOT NULL,
  position_y DECIMAL(10,2) NOT NULL,
  width DECIMAL(10,2) NOT NULL,
  height DECIMAL(10,2) NOT NULL,
  rotation DECIMAL(10,2) NOT NULL DEFAULT 0,
  color VARCHAR(50),
  z_index INT NOT NULL DEFAULT 0,
  CONSTRAINT ck_design_layers_layer_type CHECK (layer_type IN ('TEXT', 'IMAGE', 'ICON')),
  CONSTRAINT ck_design_layers_size CHECK (width > 0 AND height > 0)
);

CREATE TABLE IF NOT EXISTS ai_tryon.tryon_requests (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  customer_id UUID NOT NULL,
  design_id UUID NOT NULL REFERENCES design.designs(id),
  user_photo_url TEXT NOT NULL,
  height_cm DECIMAL(5,2),
  weight_kg DECIMAL(5,2),
  status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  error_message TEXT,
  requested_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  completed_at TIMESTAMPTZ,
  CONSTRAINT ck_tryon_requests_status CHECK (status IN ('PENDING', 'PROCESSING', 'SUCCEEDED', 'FAILED')),
  CONSTRAINT ck_tryon_requests_body_metrics CHECK (
    (height_cm IS NULL OR height_cm > 0)
    AND (weight_kg IS NULL OR weight_kg > 0)
  )
);

CREATE TABLE IF NOT EXISTS ai_tryon.tryon_results (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tryon_request_id UUID NOT NULL UNIQUE REFERENCES ai_tryon.tryon_requests(id),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  result_image_url TEXT NOT NULL,
  processing_time_ms INT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_tryon_results_processing_time CHECK (processing_time_ms IS NULL OR processing_time_ms >= 0)
);

CREATE TABLE IF NOT EXISTS ordering.orders (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  order_code VARCHAR(100) NOT NULL UNIQUE,
  customer_id UUID NOT NULL,
  total_amount DECIMAL(18,2) NOT NULL,
  payment_status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  order_status VARCHAR(50) NOT NULL DEFAULT 'PENDING_PAYMENT',
  receiver_name VARCHAR(255) NOT NULL,
  receiver_phone VARCHAR(20) NOT NULL,
  shipping_address TEXT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_orders_total_amount CHECK (total_amount >= 0),
  CONSTRAINT ck_orders_payment_status CHECK (payment_status IN ('PENDING', 'PAID', 'FAILED')),
  CONSTRAINT ck_orders_order_status CHECK (
    order_status IN (
      'PENDING_PAYMENT',
      'PAID',
      'IN_PRODUCTION',
      'SHIPPING',
      'COMPLETED',
      'CANCELLED'
    )
  )
);

CREATE TABLE IF NOT EXISTS ordering.order_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  order_id UUID NOT NULL REFERENCES ordering.orders(id),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  product_variant_id UUID NOT NULL REFERENCES catalog.product_variants(id),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  product_name_snapshot VARCHAR(255) NOT NULL,
  variant_snapshot JSONB NOT NULL,
  quantity INT NOT NULL,
  unit_price DECIMAL(18,2) NOT NULL,
  total_price DECIMAL(18,2) NOT NULL,
  CONSTRAINT ck_order_item_quantity CHECK (quantity > 0),
  CONSTRAINT ck_order_item_prices CHECK (unit_price >= 0 AND total_price >= 0)
);

CREATE TABLE IF NOT EXISTS payment.payments (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  order_id UUID NOT NULL,
  customer_id UUID NOT NULL,
  amount DECIMAL(18,2) NOT NULL,
  payment_method VARCHAR(50) NOT NULL DEFAULT 'MOCK',
  payment_status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  transaction_code VARCHAR(100),
  invoice_number VARCHAR(100),
  invoice_pdf_url TEXT,
  paid_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_payments_amount CHECK (amount >= 0),
  CONSTRAINT ck_payments_method CHECK (payment_method IN ('MOCK', 'VNPAY', 'MOMO')),
  CONSTRAINT ck_payments_status CHECK (payment_status IN ('PENDING', 'PAID', 'FAILED'))
);

CREATE TABLE IF NOT EXISTS ordering.order_status_history (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  order_id UUID NOT NULL REFERENCES ordering.orders(id),
  from_status VARCHAR(50),
  to_status VARCHAR(50) NOT NULL,
  changed_by UUID,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_order_status_history_from_status CHECK (
    from_status IS NULL OR from_status IN (
      'PENDING_PAYMENT',
      'PAID',
      'IN_PRODUCTION',
      'SHIPPING',
      'COMPLETED',
      'CANCELLED'
    )
  ),
  CONSTRAINT ck_order_status_history_to_status CHECK (
    to_status IN (
      'PENDING_PAYMENT',
      'PAID',
      'IN_PRODUCTION',
      'SHIPPING',
      'COMPLETED',
      'CANCELLED'
    )
  )
);

CREATE TABLE IF NOT EXISTS feedback.feedbacks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  customer_id UUID NOT NULL,
  order_id UUID NOT NULL REFERENCES ordering.orders(id),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  rating INT NOT NULL,
  comment TEXT,
  image_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  reviewed_by UUID,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_feedback_rating CHECK (rating BETWEEN 1 AND 5),
  CONSTRAINT ck_feedbacks_status CHECK (status IN ('PENDING', 'APPROVED', 'HIDDEN', 'REJECTED'))
);

CREATE TABLE IF NOT EXISTS content.about_us_contents (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  section_key VARCHAR(100) NOT NULL UNIQUE,
  title VARCHAR(255) NOT NULL,
  content TEXT NOT NULL,
  image_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
  updated_by UUID,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_about_us_contents_status CHECK (status IN ('DRAFT', 'PUBLISHED'))
);

-- ============================================================
-- Indexes (Postgres does NOT auto-index FK columns)
-- ============================================================

CREATE INDEX IF NOT EXISTS ix_user_roles_user_id           ON identity.user_roles(user_id);
CREATE INDEX IF NOT EXISTS ix_user_roles_role_id           ON identity.user_roles(role_id);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user_id       ON identity.refresh_tokens(user_id);

CREATE INDEX IF NOT EXISTS ix_product_variants_product_id  ON catalog.product_variants(product_id);
CREATE INDEX IF NOT EXISTS ix_product_images_product_id    ON catalog.product_images(product_id);

CREATE INDEX IF NOT EXISTS ix_designs_customer_id          ON design.designs(customer_id);
CREATE INDEX IF NOT EXISTS ix_designs_product_id           ON design.designs(product_id);
CREATE INDEX IF NOT EXISTS ix_design_layers_design_id      ON design.design_layers(design_id);

CREATE INDEX IF NOT EXISTS ix_tryon_requests_customer_id   ON ai_tryon.tryon_requests(customer_id);
CREATE INDEX IF NOT EXISTS ix_tryon_requests_design_id     ON ai_tryon.tryon_requests(design_id);
CREATE INDEX IF NOT EXISTS ix_tryon_requests_status        ON ai_tryon.tryon_requests(status);
CREATE INDEX IF NOT EXISTS ix_tryon_results_design_id      ON ai_tryon.tryon_results(design_id);

CREATE INDEX IF NOT EXISTS ix_orders_customer_id           ON ordering.orders(customer_id);
CREATE INDEX IF NOT EXISTS ix_orders_order_status          ON ordering.orders(order_status);
CREATE INDEX IF NOT EXISTS ix_order_items_order_id         ON ordering.order_items(order_id);
CREATE INDEX IF NOT EXISTS ix_order_items_design_id        ON ordering.order_items(design_id);
CREATE INDEX IF NOT EXISTS ix_order_status_history_order_id ON ordering.order_status_history(order_id);

CREATE INDEX IF NOT EXISTS ix_payments_customer_id         ON payment.payments(customer_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payments_order_id      ON payment.payments(order_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payments_transaction_code
  ON payment.payments(transaction_code)
  WHERE transaction_code IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_payments_invoice_number
  ON payment.payments(invoice_number)
  WHERE invoice_number IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_feedbacks_order_id           ON feedback.feedbacks(order_id);
CREATE INDEX IF NOT EXISTS ix_feedbacks_product_id         ON feedback.feedbacks(product_id);
CREATE INDEX IF NOT EXISTS ix_feedbacks_status             ON feedback.feedbacks(status);

-- ============================================================
-- updated_at auto-touch trigger
-- ============================================================

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DO $$
DECLARE
  t TEXT;
  tables TEXT[] := ARRAY[
    'identity.users',
    'catalog.products',
    'catalog.product_variants',
    'catalog.product_inventory',
    'design.designs',
    'ordering.orders',
    'feedback.feedbacks',
    'content.about_us_contents'
  ];
BEGIN
  FOREACH t IN ARRAY tables LOOP
    EXECUTE format(
      'DROP TRIGGER IF EXISTS trg_set_updated_at ON %s;
       CREATE TRIGGER trg_set_updated_at BEFORE UPDATE ON %s
       FOR EACH ROW EXECUTE FUNCTION set_updated_at();', t, t);
  END LOOP;
END $$;

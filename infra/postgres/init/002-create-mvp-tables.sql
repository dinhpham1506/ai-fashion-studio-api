CREATE TABLE IF NOT EXISTS identity.users (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  phone VARCHAR(20),
  avatar_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS identity.roles (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  code VARCHAR(50) NOT NULL UNIQUE,
  name VARCHAR(100) NOT NULL,
  description TEXT,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS identity.user_roles (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES identity.users(id),
  role_id UUID NOT NULL REFERENCES identity.roles(id),
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_user_roles_user_role UNIQUE (user_id, role_id)
);

CREATE TABLE IF NOT EXISTS identity.refresh_tokens (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES identity.users(id),
  token_hash TEXT NOT NULL,
  expires_at TIMESTAMP NOT NULL,
  revoked_at TIMESTAMP,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS catalog.products (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name VARCHAR(255) NOT NULL,
  description TEXT,
  base_price DECIMAL(18,2) NOT NULL,
  status VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
  created_by UUID,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS catalog.product_variants (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  sku VARCHAR(100) NOT NULL UNIQUE,
  size VARCHAR(50) NOT NULL,
  color VARCHAR(100) NOT NULL,
  material VARCHAR(100) NOT NULL,
  price_adjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
  status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS catalog.product_images (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  image_url TEXT NOT NULL,
  is_thumbnail BOOLEAN NOT NULL DEFAULT FALSE,
  sort_order INT NOT NULL DEFAULT 0,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS catalog.product_inventory (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  product_variant_id UUID NOT NULL UNIQUE REFERENCES catalog.product_variants(id),
  available_quantity INT NOT NULL DEFAULT 0,
  reserved_quantity INT NOT NULL DEFAULT 0,
  sold_quantity INT NOT NULL DEFAULT 0,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_inventory_quantities CHECK (
    available_quantity >= 0
    AND reserved_quantity >= 0
    AND sold_quantity >= 0
  )
);

CREATE TABLE IF NOT EXISTS design.designs (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  customer_id UUID NOT NULL,
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  product_variant_id UUID NOT NULL REFERENCES catalog.product_variants(id),
  name VARCHAR(255) NOT NULL,
  canvas_json JSONB NOT NULL,
  preview_image_url TEXT,
  print_file_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS design.design_layers (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  layer_type VARCHAR(50) NOT NULL,
  content TEXT,
  position_x DECIMAL(10,2) NOT NULL,
  position_y DECIMAL(10,2) NOT NULL,
  width DECIMAL(10,2) NOT NULL,
  height DECIMAL(10,2) NOT NULL,
  rotation DECIMAL(10,2) NOT NULL DEFAULT 0,
  color VARCHAR(50),
  z_index INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS ai_tryon.tryon_requests (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  customer_id UUID NOT NULL,
  design_id UUID NOT NULL REFERENCES design.designs(id),
  user_photo_url TEXT NOT NULL,
  height_cm DECIMAL(5,2),
  weight_kg DECIMAL(5,2),
  status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  error_message TEXT,
  requested_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  completed_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ai_tryon.tryon_results (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  tryon_request_id UUID NOT NULL UNIQUE REFERENCES ai_tryon.tryon_requests(id),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  result_image_url TEXT NOT NULL,
  processing_time_ms INT,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ordering.orders (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  order_code VARCHAR(100) NOT NULL UNIQUE,
  customer_id UUID NOT NULL,
  total_amount DECIMAL(18,2) NOT NULL,
  payment_status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  order_status VARCHAR(50) NOT NULL DEFAULT 'PENDING_PAYMENT',
  receiver_name VARCHAR(255) NOT NULL,
  receiver_phone VARCHAR(20) NOT NULL,
  shipping_address TEXT NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ordering.order_items (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  order_id UUID NOT NULL REFERENCES ordering.orders(id),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  product_variant_id UUID NOT NULL REFERENCES catalog.product_variants(id),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  product_name_snapshot VARCHAR(255) NOT NULL,
  variant_snapshot JSONB NOT NULL,
  quantity INT NOT NULL,
  unit_price DECIMAL(18,2) NOT NULL,
  total_price DECIMAL(18,2) NOT NULL,
  CONSTRAINT ck_order_item_quantity CHECK (quantity > 0)
);

CREATE TABLE IF NOT EXISTS payment.payments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  order_id UUID NOT NULL,
  customer_id UUID NOT NULL,
  amount DECIMAL(18,2) NOT NULL,
  payment_method VARCHAR(50) NOT NULL DEFAULT 'MOCK',
  payment_status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  transaction_code VARCHAR(100),
  invoice_number VARCHAR(100),
  invoice_pdf_url TEXT,
  paid_at TIMESTAMP,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ordering.order_status_history (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  order_id UUID NOT NULL REFERENCES ordering.orders(id),
  from_status VARCHAR(50),
  to_status VARCHAR(50) NOT NULL,
  changed_by UUID,
  note TEXT,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS feedback.feedbacks (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  customer_id UUID NOT NULL,
  order_id UUID NOT NULL REFERENCES ordering.orders(id),
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  rating INT NOT NULL,
  comment TEXT,
  image_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
  reviewed_by UUID,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_feedback_rating CHECK (rating BETWEEN 1 AND 5)
);

CREATE TABLE IF NOT EXISTS content.about_us_contents (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  section_key VARCHAR(100) NOT NULL UNIQUE,
  title VARCHAR(255) NOT NULL,
  content TEXT NOT NULL,
  image_url TEXT,
  status VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
  updated_by UUID,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

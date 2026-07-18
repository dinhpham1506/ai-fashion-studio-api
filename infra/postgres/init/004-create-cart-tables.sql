-- ============================================================
-- Cart tables for Java ordering checkout flow.
-- Cart does not reserve inventory. Inventory is reserved only
-- when the customer checks out and an order is created.
-- ============================================================

CREATE TABLE IF NOT EXISTS ordering.carts (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  customer_id UUID NOT NULL UNIQUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ordering.cart_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  cart_id UUID NOT NULL REFERENCES ordering.carts(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES catalog.products(id),
  product_variant_id UUID NOT NULL REFERENCES catalog.product_variants(id),
  design_id UUID NOT NULL REFERENCES design.designs(id),
  quantity INT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT ck_cart_items_quantity CHECK (quantity > 0),
  CONSTRAINT uq_cart_items_cart_design_variant UNIQUE (cart_id, product_id, product_variant_id, design_id)
);

CREATE INDEX IF NOT EXISTS ix_cart_items_cart_id ON ordering.cart_items(cart_id);
CREATE INDEX IF NOT EXISTS ix_cart_items_design_id ON ordering.cart_items(design_id);
CREATE INDEX IF NOT EXISTS ix_cart_items_product_variant_id ON ordering.cart_items(product_variant_id);

DO $$
DECLARE
  t TEXT;
  tables TEXT[] := ARRAY[
    'ordering.carts',
    'ordering.cart_items'
  ];
BEGIN
  FOREACH t IN ARRAY tables LOOP
    EXECUTE format(
      'DROP TRIGGER IF EXISTS trg_set_updated_at ON %s;
       CREATE TRIGGER trg_set_updated_at BEFORE UPDATE ON %s
       FOR EACH ROW EXECUTE FUNCTION set_updated_at();', t, t);
  END LOOP;
END $$;

INSERT INTO identity.roles (code, name, description)
VALUES
  ('ADMIN', 'Quan tri vien', 'Full system administration role'),
  ('STAFF', 'Nhan vien', 'Production and operation staff role'),
  ('CUSTOMER', 'Khach hang', 'Customer shopping and customization role')
ON CONFLICT (code) DO NOTHING;

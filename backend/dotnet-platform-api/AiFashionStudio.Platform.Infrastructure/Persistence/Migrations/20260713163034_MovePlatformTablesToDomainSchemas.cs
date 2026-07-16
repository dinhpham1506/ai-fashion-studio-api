using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MovePlatformTablesToDomainSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS content;
                CREATE SCHEMA IF NOT EXISTS feedback;
                CREATE SCHEMA IF NOT EXISTS identity;
                CREATE SCHEMA IF NOT EXISTS payment;
                CREATE SCHEMA IF NOT EXISTS platform;

                CREATE OR REPLACE PROCEDURE platform.normalize_id_column(target_schema text, target_table text)
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF to_regclass(format('%I.%I', target_schema, target_table)) IS NOT NULL
                       AND EXISTS (
                           SELECT 1
                           FROM information_schema.columns
                           WHERE table_schema = target_schema
                             AND table_name = target_table
                             AND column_name = 'Id'
                       ) THEN
                        EXECUTE format('ALTER TABLE %I.%I RENAME COLUMN "Id" TO id', target_schema, target_table);
                    END IF;
                END;
                $$;

                CREATE OR REPLACE PROCEDURE platform.move_or_merge_public_table(
                    target_schema text,
                    target_table text,
                    column_list text,
                    where_clause text DEFAULT 'TRUE')
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF to_regclass(format('public.%I', target_table)) IS NULL THEN
                        RETURN;
                    END IF;

                    IF to_regclass(format('%I.%I', target_schema, target_table)) IS NULL THEN
                        EXECUTE format('ALTER TABLE public.%I SET SCHEMA %I', target_table, target_schema);
                    ELSE
                        EXECUTE format(
                            'INSERT INTO %I.%I (%s) SELECT %s FROM public.%I WHERE %s ON CONFLICT DO NOTHING',
                            target_schema,
                            target_table,
                            column_list,
                            column_list,
                            target_table,
                            where_clause);
                    END IF;
                END;
                $$;

                ALTER TABLE IF EXISTS content.about_us_contents
                    ADD COLUMN IF NOT EXISTS created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;

                CALL platform.normalize_id_column('public', 'roles');
                CALL platform.normalize_id_column('public', 'users');
                CALL platform.normalize_id_column('public', 'user_roles');
                CALL platform.normalize_id_column('public', 'refresh_tokens');
                CALL platform.normalize_id_column('public', 'password_reset_otps');
                CALL platform.normalize_id_column('public', 'payment_orders');
                CALL platform.normalize_id_column('public', 'invoices');
                CALL platform.normalize_id_column('public', 'invoice_items');
                CALL platform.normalize_id_column('public', 'feedbacks');
                CALL platform.normalize_id_column('public', 'about_us_contents');

                CALL platform.normalize_id_column('identity', 'roles');
                CALL platform.normalize_id_column('identity', 'users');
                CALL platform.normalize_id_column('identity', 'user_roles');
                CALL platform.normalize_id_column('identity', 'refresh_tokens');
                CALL platform.normalize_id_column('identity', 'password_reset_otps');
                CALL platform.normalize_id_column('payment', 'payment_orders');
                CALL platform.normalize_id_column('payment', 'invoices');
                CALL platform.normalize_id_column('payment', 'invoice_items');
                CALL platform.normalize_id_column('feedback', 'feedbacks');
                CALL platform.normalize_id_column('content', 'about_us_contents');

                CALL platform.move_or_merge_public_table(
                    'identity',
                    'roles',
                    'id, code, name, description, created_at');
                CALL platform.move_or_merge_public_table(
                    'identity',
                    'users',
                    'id, email, password_hash, full_name, phone, avatar_url, status, created_at, updated_at');
                CALL platform.move_or_merge_public_table(
                    'identity',
                    'user_roles',
                    'id, user_id, role_id, created_at',
                    'EXISTS (SELECT 1 FROM identity.users target_user WHERE target_user.id = public.user_roles.user_id)
                     AND EXISTS (SELECT 1 FROM identity.roles target_role WHERE target_role.id = public.user_roles.role_id)');
                CALL platform.move_or_merge_public_table(
                    'identity',
                    'refresh_tokens',
                    'id, user_id, token_hash, expires_at, revoked_at, created_at',
                    'EXISTS (SELECT 1 FROM identity.users target_user WHERE target_user.id = public.refresh_tokens.user_id)');
                CALL platform.move_or_merge_public_table(
                    'identity',
                    'password_reset_otps',
                    'id, user_id, otp_hash, reset_token_hash, attempt_count, expires_at, reset_token_expires_at, used_at, revoked_at, created_at',
                    'EXISTS (SELECT 1 FROM identity.users target_user WHERE target_user.id = public.password_reset_otps.user_id)');
                CALL platform.move_or_merge_public_table(
                    'payment',
                    'payment_orders',
                    'id, user_id, order_id, order_code, amount, description, payment_link_id, status, gateway_reference, paid_at, cancelled_at, created_at, updated_at');
                CALL platform.move_or_merge_public_table(
                    'payment',
                    'invoices',
                    'id, order_id, payment_id, customer_id, invoice_number, total_amount, currency, status, pdf_url, issued_at, created_at');
                CALL platform.move_or_merge_public_table(
                    'payment',
                    'invoice_items',
                    'id, product_name_snapshot, variant_snapshot, quantity, unit_price, created_at, invoice_id');
                CALL platform.move_or_merge_public_table(
                    'feedback',
                    'feedbacks',
                    'id, customer_id, order_id, product_id, rating, comment, image_url, status, reviewed_by, created_at, updated_at');
                CALL platform.move_or_merge_public_table(
                    'content',
                    'about_us_contents',
                    'id, section_key, title, content, image_url, status, updated_by, created_at, updated_at');

                ALTER TABLE IF EXISTS content.about_us_contents
                    ADD COLUMN IF NOT EXISTS created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_refresh_tokens_token_hash"
                    ON identity.refresh_tokens (token_hash);
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_feedbacks_customer_id_order_id_product_id"
                    ON feedback.feedbacks (customer_id, order_id, product_id);

                DROP TABLE IF EXISTS public.invoice_items CASCADE;
                DROP TABLE IF EXISTS public.invoices CASCADE;
                DROP TABLE IF EXISTS public.payment_orders CASCADE;
                DROP TABLE IF EXISTS public.password_reset_otps CASCADE;
                DROP TABLE IF EXISTS public.refresh_tokens CASCADE;
                DROP TABLE IF EXISTS public.user_roles CASCADE;
                DROP TABLE IF EXISTS public.feedbacks CASCADE;
                DROP TABLE IF EXISTS public.about_us_contents CASCADE;
                DROP TABLE IF EXISTS public.users CASCADE;
                DROP TABLE IF EXISTS public.roles CASCADE;

                DROP PROCEDURE IF EXISTS platform.move_or_merge_public_table(text, text, text, text);
                DROP PROCEDURE IF EXISTS platform.move_or_merge_public_table(text, text, text);
                DROP PROCEDURE IF EXISTS platform.normalize_id_column(text, text);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                schema: "identity",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "user_roles",
                schema: "identity",
                newName: "user_roles");

            migrationBuilder.RenameTable(
                name: "roles",
                schema: "identity",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                schema: "identity",
                newName: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "payment_orders",
                schema: "payment",
                newName: "payment_orders");

            migrationBuilder.RenameTable(
                name: "password_reset_otps",
                schema: "identity",
                newName: "password_reset_otps");

            migrationBuilder.RenameTable(
                name: "invoices",
                schema: "payment",
                newName: "invoices");

            migrationBuilder.RenameTable(
                name: "invoice_items",
                schema: "payment",
                newName: "invoice_items");

            migrationBuilder.RenameTable(
                name: "feedbacks",
                schema: "feedback",
                newName: "feedbacks");

            migrationBuilder.RenameTable(
                name: "about_us_contents",
                schema: "content",
                newName: "about_us_contents");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "user_roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "refresh_tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "payment_orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "password_reset_otps",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "invoices",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "invoice_items",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "feedbacks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "about_us_contents",
                newName: "Id");
        }
    }
}

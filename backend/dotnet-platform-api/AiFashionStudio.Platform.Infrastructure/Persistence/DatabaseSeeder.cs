using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Content.Entities;
using AiFashionStudio.Platform.Domain.Content.Enums;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using AiFashionStudio.Platform.Domain.Invoice.Entities;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with initial roles, users, about-us content, payment orders, and an invoice.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = Role.Create(RoleName.Admin, "Administrator", "Administrator role with full access");
            var staffRole = Role.Create(RoleName.Staff, "Staff", "Staff role");
            var customerRole = Role.Create(RoleName.Customer, "Customer", "Customer role");

            context.Roles.AddRange(adminRole, staffRole, customerRole);
            await context.SaveChangesAsync();
        }

        // 2. Seed Users
        if (!await context.Users.AnyAsync())
        {
            var roles = await context.Roles.ToListAsync();
            var adminRole = roles.First(r => r.Code == RoleName.Admin);
            var staffRole = roles.First(r => r.Code == RoleName.Staff);
            var customerRole = roles.First(r => r.Code == RoleName.Customer);

            var passwordHash = passwordHasher.Hash("Password123!");

            var adminUser = User.Register("admin@aifashion.com", passwordHash, "System Admin", "0900000001");
            var staffUser = User.Register("staff@aifashion.com", passwordHash, "System Staff", "0900000002");
            var customerUser = User.Register("customer@aifashion.com", passwordHash, "Test Customer", "0900000003");
            var customerUser2 = User.Register("customer2@aifashion.com", passwordHash, "Second Customer", "0900000004");

            adminUser.AssignRole(adminRole);
            staffUser.AssignRole(staffRole);
            customerUser.AssignRole(customerRole);
            customerUser2.AssignRole(customerRole);

            context.Users.AddRange(adminUser, staffUser, customerUser, customerUser2);
            await context.SaveChangesAsync();
        }

        // Get admin user and customer user for subsequent seedings
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@aifashion.com");
        var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@aifashion.com");
        var customer2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer2@aifashion.com");

        // 3. Seed AboutUsContent
        if (!await context.AboutUsContents.AnyAsync() && admin != null)
        {
            var content1 = AboutUsContent.Create("INTRODUCTION", "Welcome to AI Fashion Studio", "We bring AI into fashion designing.", "https://example.com/image1.jpg", AboutUsStatus.Published, admin.Id);
            var content2 = AboutUsContent.Create("MISSION", "Our Mission", "To revolutionize the fashion industry.", null, AboutUsStatus.Published, admin.Id);
            // Section DRAFT: public GET /api/about-us không được trả về section này
            var content3 = AboutUsContent.Create("HOW_IT_WORKS", "How It Works", "Pick a shirt, design it, try it on with AI, then order.", null, AboutUsStatus.Draft, admin.Id);

            context.AboutUsContents.AddRange(content1, content2, content3);
            await context.SaveChangesAsync();
        }

        // 4. Seed PaymentOrders & Invoices
        if (!await context.PaymentOrders.AnyAsync() && customer != null)
        {
            // Seed a pending payment order
            var pendingOrder = PaymentOrder.Create(customer.Id, 10001, 500000, "Thanh toán đơn hàng 10001");

            // Seed a paid payment order and its invoice
            var paidOrder = PaymentOrder.Create(customer.Id, 10002, 1200000, "Thanh toán đơn hàng 10002");
            paidOrder.AttachPaymentLink("https://pay.example.com/10002");
            paidOrder.MarkPaid("GW-REF-12345");

            // Cancelled + expired orders để test các nhánh trạng thái
            var cancelledOrder = PaymentOrder.Create(customer.Id, 10003, 300000, "Thanh toán đơn hàng 10003");
            cancelledOrder.AttachPaymentLink("https://pay.example.com/10003");
            cancelledOrder.Cancel();

            var expiredOrder = PaymentOrder.Create(customer.Id, 10004, 450000, "Thanh toán đơn hàng 10004");
            expiredOrder.AttachPaymentLink("https://pay.example.com/10004");
            expiredOrder.MarkExpired();

            context.PaymentOrders.AddRange(pendingOrder, paidOrder, cancelledOrder, expiredOrder);

            // Order của customer2 — dùng để test rule "customer chỉ xem payment/invoice của mình"
            if (customer2 != null)
            {
                var otherCustomerOrder = PaymentOrder.Create(customer2.Id, 20001, 750000, "Thanh toán đơn hàng 20001");
                context.PaymentOrders.Add(otherCustomerOrder);
            }

            await context.SaveChangesAsync();

            // Seed invoice for the paid order
            if (!await context.Invoices.AnyAsync())
            {
                var invoiceItems = new List<InvoiceItem>
                {
                    InvoiceItem.Create("Áo thun AI", "Size L", 2, 300000),
                    InvoiceItem.Create("Quần Jean AI", "Size 32", 1, 600000)
                };
                
                // Assuming OrderId is just a new Guid for mock data, or mapping to something else.
                var dummyOrderId = Guid.NewGuid();
                var invoice = Invoice.Issue(dummyOrderId, paidOrder.Id, customer.Id, "INV-10002", "VND", invoiceItems);
                invoice.AttachPdf("https://example.com/invoices/inv-10002.pdf");

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();
            }
        }
    }
}

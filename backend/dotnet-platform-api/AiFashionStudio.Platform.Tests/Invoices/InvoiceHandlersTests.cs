using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Invoices.Commands.CreateInvoice;
using AiFashionStudio.Platform.Application.Invoices.Commands.GenerateInvoicePdf;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceById;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceItems;
using AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoicePdf;
using AiFashionStudio.Platform.Domain.Invoice.Entities;
using AiFashionStudio.Platform.Tests.Common;
using Xunit;

namespace AiFashionStudio.Platform.Tests.Invoices;

public class InvoiceHandlersTests
{
    [Fact]
    public async Task CreateInvoice_Should_Create_Invoice_When_Order_Has_No_Invoice()
    {
        var repository = new FakeInvoiceRepository { IssuedTodayCount = 4 };
        var handler = new CreateInvoiceCommandHandler(repository);

        var invoiceId = await handler.Handle(
            new CreateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "custom shirt", 250000),
            CancellationToken.None);

        var invoice = Assert.Single(repository.Items);
        Assert.Equal(invoice.Id, invoiceId);
        Assert.StartsWith("INV", invoice.InvoiceNumber);
        Assert.EndsWith("0005", invoice.InvoiceNumber);
        Assert.Equal(250000, invoice.TotalAmount);
    }

    [Fact]
    public async Task CreateInvoice_Should_Return_Existing_Invoice_When_Order_Already_Has_Invoice()
    {
        var orderId = Guid.NewGuid();
        var existing = CreateInvoice(orderId, Guid.NewGuid(), "INVEXISTING");
        var repository = new FakeInvoiceRepository(existing);
        var handler = new CreateInvoiceCommandHandler(repository);

        var invoiceId = await handler.Handle(
            new CreateInvoiceCommand(orderId, Guid.NewGuid(), Guid.NewGuid(), "another", 100000),
            CancellationToken.None);

        Assert.Equal(existing.Id, invoiceId);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task GenerateInvoicePdf_Should_Attach_Pdf_Url_When_Not_Ready()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), Guid.NewGuid(), "INVPDF0001");
        var repository = new FakeInvoiceRepository(invoice);
        var storage = new FakeFileStorage("https://storage.test/invoices/INVPDF0001.pdf");
        var handler = new GenerateInvoicePdfCommandHandler(
            repository,
            new FakeInvoicePdfGenerator(),
            storage);

        await handler.Handle(new GenerateInvoicePdfCommand(invoice.Id), CancellationToken.None);

        Assert.Equal("INVPDF0001.pdf", invoice.PdfUrl);
        Assert.Equal(1, storage.UploadCount);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task GetInvoiceById_Should_Reject_Non_Owner_When_Not_Staff()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), Guid.NewGuid(), "INVACCESS0001");
        var handler = new GetInvoiceByIdQueryHandler(new FakeInvoiceRepository(invoice));

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetInvoiceByIdQuery(Guid.NewGuid(), false, invoice.Id), CancellationToken.None));

        Assert.Equal("INVOICE_ACCESS_DENIED", exception.Errors.Single().Code);
    }

    [Fact]
    public async Task GetInvoicePdf_Should_Return_Pdf_For_Owner()
    {
        var ownerId = Guid.NewGuid();
        var invoice = CreateInvoice(Guid.NewGuid(), ownerId, "INVOWNER0001");
        invoice.AttachPdf("INVOWNER0001.pdf");
        var handler = new GetInvoicePdfQueryHandler(
            new FakeInvoiceRepository(invoice),
            new FakeFileStorage("https://storage.test/presigned/INVOWNER0001.pdf"));

        var response = await handler.Handle(new GetInvoicePdfQuery(ownerId, false, invoice.Id), CancellationToken.None);

        Assert.Equal("INVOWNER0001", response.InvoiceNumber);
        Assert.Equal("https://storage.test/presigned/INVOWNER0001.pdf", response.PdfUrl);
    }

    [Fact]
    public async Task GetInvoiceItems_Should_Return_Items_For_Staff()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), Guid.NewGuid(), "INVITEM0001");
        var handler = new GetInvoiceItemsQueryHandler(new FakeInvoiceRepository(invoice));

        var response = await handler.Handle(new GetInvoiceItemsQuery(Guid.NewGuid(), true, invoice.Id), CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal("custom shirt", item.ProductNameSnapshot);
        Assert.Equal(250000, item.TotalPrice);
    }

    private static Invoice CreateInvoice(Guid orderId, Guid customerId, string invoiceNumber)
        => Invoice.Issue(
            orderId,
            Guid.NewGuid(),
            customerId,
            invoiceNumber,
            "VND",
            [InvoiceItem.Create("custom shirt", null, 1, 250000)]);

    private sealed class FakeInvoiceRepository : InMemoryRepository<Invoice>, IInvoiceRepository
    {
        public FakeInvoiceRepository(params Invoice[] invoices)
        {
            Store.AddRange(invoices);
        }

        public int IssuedTodayCount { get; init; }

        public Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(invoice => invoice.OrderId == orderId));

        public Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.Any(invoice => invoice.OrderId == orderId));

        public Task<int> CountIssuedTodayAsync(DateOnly issuedDate, CancellationToken cancellationToken = default)
            => Task.FromResult(IssuedTodayCount);

        public Task<Invoice> IssueForOrderAsync(
            Guid orderId,
            Guid paymentId,
            Guid customerId,
            string description,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            var existingInvoice = Store.FirstOrDefault(invoice => invoice.OrderId == orderId);
            if (existingInvoice is not null)
            {
                return Task.FromResult(existingInvoice);
            }

            var invoice = Invoice.Issue(
                orderId,
                paymentId,
                customerId,
                $"INV{DateTime.UtcNow:yyyyMMdd}{IssuedTodayCount + Store.Count + 1:D4}",
                "VND",
                [InvoiceItem.Create(description, null, 1, amount)]);

            Store.Add(invoice);
            return Task.FromResult(invoice);
        }
    }

    private sealed class FakeInvoicePdfGenerator : IInvoicePdfGenerator
    {
        public byte[] Generate(Invoice invoice) => [1, 2, 3];
    }

    private sealed class FakeFileStorage : IFileStorage
    {
        private readonly string _url;

        public FakeFileStorage(string url)
        {
            _url = url;
        }

        public int UploadCount { get; private set; }

        public Task<string> UploadAsync(string bucket, string objectName, byte[] content, string contentType, CancellationToken cancellationToken = default)
        {
            UploadCount++;
            return Task.FromResult(_url);
        }

        public Task<string> GetTemporaryUrlAsync(string bucket, string objectName, TimeSpan expiresIn, CancellationToken cancellationToken = default)
            => Task.FromResult(_url);
    }
}

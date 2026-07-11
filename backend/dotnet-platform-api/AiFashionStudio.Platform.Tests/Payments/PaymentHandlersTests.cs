using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Models;
using AiFashionStudio.Platform.Application.Payments.Commands;
using AiFashionStudio.Platform.Application.Payments.Commands.CancelPayment;
using AiFashionStudio.Platform.Application.Payments.Commands.CreatePayment;
using AiFashionStudio.Platform.Application.Payments.Queries.GetPaymentStatus;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using AiFashionStudio.Platform.Tests.Common;
using Xunit;

namespace AiFashionStudio.Platform.Tests.Payments;

public class PaymentHandlersTests
{
    [Fact]
    public async Task CreatePayment_Should_Save_Order_And_Attach_Payment_Link()
    {
        var repository = new FakePaymentOrderRepository();
        var gateway = new FakePaymentGatewayService();
        var handler = new CreatePaymentCommandHandler(repository, gateway);

        var response = await handler.Handle(
            new CreatePaymentCommand(Guid.NewGuid(), 150000, "custom shirt"),
            CancellationToken.None);

        var order = Assert.Single(repository.Items);
        Assert.Equal(150000, response.Amount);
        Assert.Equal("PENDING", response.Status);
        Assert.Equal("checkout-url", response.CheckoutUrl);
        Assert.Equal("payment-link-id", order.PaymentLinkId);
        Assert.Equal(1, gateway.CreatePaymentLinkCount);
    }

    [Fact]
    public async Task CancelPayment_Should_Cancel_Pending_Order()
    {
        var userId = Guid.NewGuid();
        var order = PaymentOrder.Create(userId, 123, 100000, "order");
        var repository = new FakePaymentOrderRepository(order);
        var gateway = new FakePaymentGatewayService();
        var handler = new CancelPaymentCommandHandler(gateway, repository);

        await handler.Handle(new CancelPaymentCommand(userId, 123), CancellationToken.None);

        Assert.Equal("CANCELLED", order.Status.ToString().ToUpperInvariant());
        Assert.Equal(1, gateway.CancelPaymentLinkCount);
        Assert.Equal(2, repository.SaveChangesCount);
    }

    [Fact]
    public async Task CancelPayment_Should_Reject_Non_Pending_Order()
    {
        var userId = Guid.NewGuid();
        var order = PaymentOrder.Create(userId, 123, 100000, "order");
        order.MarkPaid("ref");
        var handler = new CancelPaymentCommandHandler(
            new FakePaymentGatewayService(),
            new FakePaymentOrderRepository(order));

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new CancelPaymentCommand(userId, 123), CancellationToken.None));

        Assert.Equal("PAYMENT_NOT_CANCELLABLE", exception.Errors.Single().Code);
    }

    [Fact]
    public async Task GetPaymentStatus_Should_Return_Owner_Payment_Status()
    {
        var userId = Guid.NewGuid();
        var order = PaymentOrder.Create(userId, 456, 90000, "order");
        order.MarkPaid("ref");
        var handler = new GetPaymentStatusQueryHandler(new FakePaymentOrderRepository(order));

        var response = await handler.Handle(new GetPaymentStatusQuery(userId, 456), CancellationToken.None);

        Assert.Equal(order.Id, response.OrderId);
        Assert.Equal("PAID", response.Status);
        Assert.NotNull(response.PaidAt);
    }

    private sealed class FakePaymentOrderRepository : InMemoryRepository<PaymentOrder>, IPaymentOrderRepository
    {
        public FakePaymentOrderRepository(params PaymentOrder[] orders)
        {
            Store.AddRange(orders);
        }

        public Task<PaymentOrder?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(order => order.OrderCode == orderCode));

        public Task<PaymentOrder?> GetByOrderCodeAndUserIdAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(order => order.OrderCode == orderCode && order.UserId == userId));
    }

    private sealed class FakePaymentGatewayService : IPaymentGatewayService
    {
        public int CreatePaymentLinkCount { get; private set; }
        public int CancelPaymentLinkCount { get; private set; }

        public Task<PaymentLinkResponse> CreatePaymentLinkAsync(PaymentLinkRequest request, CancellationToken cancellationToken = default)
        {
            CreatePaymentLinkCount++;
            return Task.FromResult(new PaymentLinkResponse("payment-link-id", "checkout-url", "qr-code"));
        }

        public Task<string> GetPaymentLinkStatusAsync(long orderCode, CancellationToken cancellationToken = default)
            => Task.FromResult("PENDING");

        public Task CancelPaymentLinkAsync(long orderCode, string? reason = null, CancellationToken cancellationToken = default)
        {
            CancelPaymentLinkCount++;
            return Task.CompletedTask;
        }

        public Task<PaymentWebhookData> VerifyWebhookAsync(string rawJsonBody)
            => Task.FromResult(new PaymentWebhookData(0, 0, "ref", false));
    }
}

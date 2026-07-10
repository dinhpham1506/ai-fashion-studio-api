using System.Linq.Expressions;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Feedbacks.Commands.ModerateFeedback;
using AiFashionStudio.Platform.Application.Feedbacks.Commands.SubmitFeedback;
using AiFashionStudio.Platform.Application.Feedbacks.Queries.GetFeedbacksForModeration;
using AiFashionStudio.Platform.Application.Feedbacks.Queries.GetPublicFeedbacks;
using AiFashionStudio.Platform.Domain.Feedback.Entities;
using AiFashionStudio.Platform.Domain.Feedback.Enums;
using Xunit;

namespace AiFashionStudio.Platform.Tests.Feedbacks;

public class FeedbackHandlersTests
{
    [Fact]
    public async Task SubmitFeedback_Should_Create_Pending_Feedback_When_Order_Is_Completed()
    {
        var repository = new FakeFeedbackRepository
        {
            Eligibility = new FeedbackOrderEligibility
            {
                OrderExists = true,
                IsCompleted = true,
                ProductBelongsToOrder = true
            }
        };
        var storage = new FakeFileStorage("https://storage.test/feedbacks/image.png");
        var handler = new SubmitFeedbackCommandHandler(repository, storage);

        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "  Quality is great  ",
            [1, 2, 3],
            "image/png",
            "image.png");

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Single(repository.AddedFeedbacks);
        Assert.Equal("PENDING", response.Status);
        Assert.Equal("Quality is great", response.Comment);
        Assert.Equal("https://storage.test/feedbacks/image.png", response.ImageUrl);
        Assert.Equal(1, storage.UploadCount);
    }

    [Fact]
    public async Task SubmitFeedback_Should_Reject_When_Order_Is_Not_Completed()
    {
        var repository = new FakeFeedbackRepository
        {
            Eligibility = new FeedbackOrderEligibility
            {
                OrderExists = true,
                IsCompleted = false,
                ProductBelongsToOrder = true
            }
        };
        var storage = new FakeFileStorage();
        var handler = new SubmitFeedbackCommandHandler(repository, storage);

        var command = ValidSubmitCommand();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("ORDER_NOT_COMPLETED", exception.Errors.Single().Code);
        Assert.Empty(repository.AddedFeedbacks);
        Assert.Equal(0, storage.UploadCount);
    }

    [Fact]
    public async Task SubmitFeedback_Should_Reject_Duplicate_Feedback_For_Same_Order_Product()
    {
        var repository = new FakeFeedbackRepository
        {
            Eligibility = new FeedbackOrderEligibility
            {
                OrderExists = true,
                IsCompleted = true,
                ProductBelongsToOrder = true
            },
            FeedbackAlreadyExists = true
        };
        var handler = new SubmitFeedbackCommandHandler(repository, new FakeFileStorage());

        var command = ValidSubmitCommand();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("FEEDBACK_ALREADY_EXISTS", exception.Errors.Single().Code);
        Assert.Empty(repository.AddedFeedbacks);
    }

    [Fact]
    public async Task SubmitFeedback_Should_Reject_Product_Not_In_Order()
    {
        var repository = new FakeFeedbackRepository
        {
            Eligibility = new FeedbackOrderEligibility
            {
                OrderExists = true,
                IsCompleted = true,
                ProductBelongsToOrder = false
            }
        };
        var handler = new SubmitFeedbackCommandHandler(repository, new FakeFileStorage());

        var exception = await Assert.ThrowsAsync<AppValidationException>(
            () => handler.Handle(ValidSubmitCommand(), CancellationToken.None));

        Assert.Equal("PRODUCT_NOT_IN_ORDER", exception.Errors.Single().Code);
        Assert.Empty(repository.AddedFeedbacks);
    }

    [Fact]
    public async Task ModerateFeedback_Should_Update_Status_And_Reviewer()
    {
        var reviewerId = Guid.NewGuid();
        var feedback = Feedback.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Good", null);
        var repository = new FakeFeedbackRepository { ExistingFeedback = feedback };
        var handler = new ModerateFeedbackCommandHandler(repository);

        var response = await handler.Handle(
            new ModerateFeedbackCommand(feedback.Id, "APPROVED", reviewerId),
            CancellationToken.None);

        Assert.Equal("APPROVED", response.Status);
        Assert.Equal(reviewerId, response.ReviewedBy);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task ModerateFeedback_Should_Return_NotFound_For_Missing_Feedback()
    {
        var repository = new FakeFeedbackRepository();
        var handler = new ModerateFeedbackCommandHandler(repository);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new ModerateFeedbackCommand(Guid.NewGuid(), "HIDDEN", Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("FEEDBACK_NOT_FOUND", exception.Errors.Single().Code);
    }

    [Fact]
    public async Task GetPublicFeedbacks_Should_Request_Only_Public_Feedbacks()
    {
        var productId = Guid.NewGuid();
        var repository = new FakeFeedbackRepository
        {
            QueryItems =
            [
                new FeedbackListItemProjection
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CustomerName = "Customer",
                    OrderId = Guid.NewGuid(),
                    ProductId = productId,
                    Rating = 5,
                    Status = FeedbackStatus.Approved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            ],
            QueryTotalCount = 1
        };
        var handler = new GetPublicFeedbacksQueryHandler(repository);

        var result = await handler.Handle(new GetPublicFeedbacksQuery(productId, 2, 10), CancellationToken.None);

        Assert.True(repository.LastOnlyPublic);
        Assert.Null(repository.LastStatus);
        Assert.Equal(productId, repository.LastProductId);
        Assert.Equal(10, repository.LastSkip);
        Assert.Equal(10, repository.LastTake);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("APPROVED", result.Items.Single().Status);
    }

    [Fact]
    public async Task GetFeedbacksForModeration_Should_Parse_Status_Filter()
    {
        var repository = new FakeFeedbackRepository { QueryTotalCount = 3 };
        var handler = new GetFeedbacksForModerationQueryHandler(repository);

        var result = await handler.Handle(
            new GetFeedbacksForModerationQuery("PENDING", null, 1, 20),
            CancellationToken.None);

        Assert.False(repository.LastOnlyPublic);
        Assert.Equal(FeedbackStatus.Pending, repository.LastStatus);
        Assert.Equal(0, repository.LastSkip);
        Assert.Equal(20, repository.LastTake);
        Assert.Equal(3, result.TotalCount);
    }

    private static SubmitFeedbackCommand ValidSubmitCommand()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            4,
            "Nice shirt",
            null,
            null,
            null);

    private sealed class FakeFileStorage : IFileStorage
    {
        private readonly string _url;

        public FakeFileStorage(string url = "https://storage.test/feedbacks/default.png")
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

    private sealed class FakeFeedbackRepository : IFeedbackRepository
    {
        public FeedbackOrderEligibility? Eligibility { get; init; }
        public bool FeedbackAlreadyExists { get; init; }
        public Feedback? ExistingFeedback { get; init; }
        public IReadOnlyList<FeedbackListItemProjection> QueryItems { get; init; } = [];
        public int QueryTotalCount { get; init; }
        public List<Feedback> AddedFeedbacks { get; } = [];
        public int SaveChangesCount { get; private set; }
        public FeedbackStatus? LastStatus { get; private set; }
        public Guid? LastProductId { get; private set; }
        public bool LastOnlyPublic { get; private set; }
        public int LastSkip { get; private set; }
        public int LastTake { get; private set; }

        public Task<bool> ExistsByCustomerOrderProductAsync(Guid customerId, Guid orderId, Guid productId, CancellationToken cancellationToken = default)
            => Task.FromResult(FeedbackAlreadyExists);

        public Task<FeedbackOrderEligibility?> GetOrderEligibilityAsync(Guid customerId, Guid orderId, Guid productId, CancellationToken cancellationToken = default)
            => Task.FromResult(Eligibility);

        public Task<Feedback?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistingFeedback?.Id == id ? ExistingFeedback : null);

        public Task AddAsync(Feedback entity, CancellationToken cancellationToken = default)
        {
            AddedFeedbacks.Add(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<FeedbackListItemProjection>> GetFeedbacksAsync(FeedbackStatus? status, Guid? productId, bool onlyPublic, int skip, int take, CancellationToken cancellationToken = default)
        {
            LastStatus = status;
            LastProductId = productId;
            LastOnlyPublic = onlyPublic;
            LastSkip = skip;
            LastTake = take;
            return Task.FromResult(QueryItems);
        }

        public Task<int> CountFeedbacksAsync(FeedbackStatus? status, Guid? productId, bool onlyPublic, CancellationToken cancellationToken = default)
            => Task.FromResult(QueryTotalCount);

        public Task<List<Feedback>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new List<Feedback>());

        public Task<List<Feedback>> FindAsync(Expression<Func<Feedback, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<Feedback>());

        public Task<Feedback?> FirstOrDefaultAsync(Expression<Func<Feedback, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<Feedback?>(null);

        public Task<bool> AnyAsync(Expression<Func<Feedback, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<int> CountAsync(Expression<Func<Feedback, bool>>? predicate = null, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task AddRangeAsync(IEnumerable<Feedback> entities, CancellationToken cancellationToken = default)
        {
            AddedFeedbacks.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Feedback entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Feedback entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

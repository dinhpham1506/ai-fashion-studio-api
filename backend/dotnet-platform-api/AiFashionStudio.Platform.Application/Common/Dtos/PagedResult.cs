namespace AiFashionStudio.Platform.Application.Common.Dtos;

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

using BasicBuildingBlocks.Extensions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BasicBuildingBlocks.Collections;

public sealed record PagedRequest(int PageNumber, int PageSize);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount
)
{
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map)
        => new([.. Items.Select(map)], Page, PageSize, TotalCount);
};

public static class Extension
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, 
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        query = query.Paginate(pageNumber, pageSize);

        var count = await query.LongCountAsync(ct);
        var items = await query.ToListAsync(ct);

        return new PagedResult<T>(items, pageNumber, pageSize, count);
    }

    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> query, 
        int pageNumber,
        int pageSize)
    {
        query = query.Paginate(pageNumber, pageSize);

        var count =  query.LongCount();
        var items = query.ToList();

        return new PagedResult<T>(items, pageNumber, pageSize, count);
    }
}
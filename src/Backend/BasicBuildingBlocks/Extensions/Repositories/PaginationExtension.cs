namespace BasicBuildingBlocks.Extensions.Repositories;

public static class PaginationExtension
{
    /// <summary>
    /// Applies pagination to an EF Core query.
    /// </summary>
    /// <param name="pageNumber">Zero-based page index (0 = first page).</param>
    /// <param name="pageSize">Number of items per page. Must be greater than 0.</param>
    /// <returns>A query with <c>Skip</c>/<c>Take</c> applied.</returns>

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, 
        int pageNumber,
        int pageSize)
    {
        if(pageNumber < 0)
            throw new ArgumentException("Номер страницы должен быть больше или равен нулю.", nameof(pageNumber));

        if(pageSize < 0)
            throw new ArgumentException("Размер страницы должен быть больше или равен нулю.", nameof(pageSize));

        if(pageSize == 0 && pageNumber == 0)
            return query;

        return query.Skip(pageNumber * pageSize).Take(pageSize);
    }
}

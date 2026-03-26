using SearchService.Models;

namespace SearchService.Data.Repositories;

public sealed class SearchListingCandidateSet
{
    public IReadOnlyCollection<SearchListing> Items { get; init; } = [];
    public long TotalCount { get; init; }
}

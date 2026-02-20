using ListingService.Application.Common.Lookups.Abstractions;
using Microsoft.EntityFrameworkCore;
using BasicBuildingBlocks.Pages;
using FluentResults;
using AutoMapper;

namespace ListingService.Application.Common.Lookups;

public class LookupHandler<Lookup, LookupDto>(IRepositoryManager repositoryManager, 
    IMapper mapper) : ILookupHandler<Lookup, LookupDto> where Lookup : class where LookupDto : class
{
    public async Task<Result<PagedResult<LookupDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await repositoryManager
            .GetLookup<Lookup>()
            .GetAllAsync<Lookup>(cancellationToken);

        return Result.Ok(result.Map(x => mapper.Map<LookupDto>(x)));
    }

    public async Task<Result> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var existing = await repositoryManager.GetLookup<Lookup>()
            .GetByNameAsync(name, cancellationToken);

        if (existing is not null)
            return Result.Fail($"{typeof(Lookup).Name} with the same name already exists.")
                .WithError("Conflict");

        await repositoryManager.GetLookup<Lookup>()
            .AddAsync(mapper.Map<Lookup>(new { Name = name }), cancellationToken);

        try
        {
            await repositoryManager.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail($"Failed to create {typeof(Lookup).Name} type.")
                .WithError(ex.Message);
        }
    }

    public async Task<Result> DeleteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var result = await repositoryManager.GetLookup<Lookup>()
            .DeleteByNameAsync(name, cancellationToken);

        return result
            ? Result.Ok()
            : Result.Fail($"{typeof(Lookup).Name} not found or could not be deleted.");
    }
}
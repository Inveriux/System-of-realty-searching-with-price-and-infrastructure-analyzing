using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups.Abstractions;
using ListingService.Application.Contracts.Cache;
using ListingService.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace ListingService.Application.Common.Lookups;

public class LookupHandler<TLookup, TLookupDto>(IRepositoryManager repositoryManager, 
    IMapper mapper, ILookupCache lookupCache) : ILookupHandler<TLookup, TLookupDto> 
    where TLookup : class, ILookupEntity where TLookupDto : class, ILookupDto
{
    public async Task<Result<List<TLookupDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await lookupCache.GetAllAsync<TLookup>(typeof(TLookup).Name, cancellationToken);
        return Result.Ok(mapper.Map<List<TLookupDto>>(result));
    }

    public async Task<Result> CreateAsync(TLookupDto lookupDto, CancellationToken cancellationToken)
    {
        var existing = await repositoryManager.GetLookup<TLookup>()
            .GetByNameAsync(lookupDto.Name, cancellationToken);

        if (existing is not null)
        {
            return Result.Conflict(
                $"{typeof(TLookup).Name} with the same name already exists.",
                [new ResultError($"{typeof(TLookup).Name} with the same name already exists.", 0, nameof(ILookupDto.Name))]);
        }

        await repositoryManager.GetLookup<TLookup>()
            .AddAsync(mapper.Map<TLookup>(lookupDto), cancellationToken);

        try
        {
            await repositoryManager.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(
                $"Failed to create {typeof(TLookup).Name} type.",
                [new ResultError(ex.Message, 0, nameof(ILookupDto.Name))]);
        }
        finally
        {
            lookupCache.Invalidate(typeof(TLookup).Name);
        }
    }

    public async Task<Result> DeleteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var isDeleted = await repositoryManager.GetLookup<TLookup>()
            .DeleteByNameAsync(name, cancellationToken);

        if (!isDeleted)
        {
            return Result.NotFound(
                $"{typeof(TLookup).Name} not found.",
                [new ResultError($"{typeof(TLookup).Name} not found.", 0, nameof(ILookupDto.Name))]);
        }

        try
        {
            await repositoryManager.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(
                $"{typeof(TLookup).Name} not found or could not be deleted.",
                [new ResultError(ex.Message, 0, nameof(ILookupDto.Name))]);
        }
        finally
        {
            lookupCache.Invalidate(typeof(TLookup).Name);
        }
    }
}

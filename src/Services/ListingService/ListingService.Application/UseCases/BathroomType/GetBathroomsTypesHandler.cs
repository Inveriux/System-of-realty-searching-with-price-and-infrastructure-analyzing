using ListingService.Application.DTOs.BathroomType;
using AutoMapper;
using MediatR;
using BasicBuildingBlocks.Pages;

using BathroomTypeModel = ListingService.Domain.Models.BathroomType;  //TODO BRUH

namespace ListingService.Application.UseCases.BathroomType;

public record GetBathroomTypesQuery() : IRequest<PagedResult<BathroomTypeDto>>;

public class GetBathroomTypesHandler(IRepositoryManager repositoryManager, IMapper mapper)
    : IRequestHandler<GetBathroomTypesQuery, PagedResult<BathroomTypeDto>>
{
    public async Task<PagedResult<BathroomTypeDto>> Handle(GetBathroomTypesQuery request, 
        CancellationToken cancellationToken)
    {
        var result = await repositoryManager
            .GetLookup<BathroomTypeModel>()
            .GetAllAsync<BathroomTypeModel>(cancellationToken);

        System.Console.WriteLine(result.Items[0].TypeName);

        return result.Map(x => mapper.Map<BathroomTypeDto>(x));
    }
}
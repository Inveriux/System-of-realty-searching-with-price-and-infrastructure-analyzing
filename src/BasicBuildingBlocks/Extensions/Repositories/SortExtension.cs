using Microsoft.EntityFrameworkCore;

namespace BasicBuildingBlocks.Extensions.Repositories;

public enum SortOrder : byte
{
    Ascending,
    Descending
}

//TODO Add sorting by multiple fields
public static class SortExtension
{

}
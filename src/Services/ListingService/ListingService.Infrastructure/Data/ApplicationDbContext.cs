using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;
using System.Reflection;

namespace ListingService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
#region Listings Information

    public DbSet<Listing> Listings { get; set; }
    public DbSet<ListingMedia> ListingMedia { get; set; }
    public DbSet<ListingPriceHistory> ListingPriceHistories { get; set; }

#endregion

#region Listing details

    public DbSet<ApartmentDetails> ApartmentDetails { get; set; }
    public DbSet<HouseDetails> HouseDetails { get; set; }
    public DbSet<LandDetails> LandDetails { get; set; }
    public DbSet<OfficeDetails> OfficeDetails { get; set; }
    public DbSet<WarehouseDetails> WarehouseDetails { get; set; }

#endregion

#region Lookup tables

    public DbSet<ListingType> ListingTypes { get; set; }
    public DbSet<BathroomType> BathroomTypes { get; set; }
    public DbSet<BuildingMaterial> BuildingMaterials { get; set; }
    public DbSet<BuildingType> BuildingTypes { get; set; }
    public DbSet<ClassType> ClassTypes { get; set; }
    public DbSet<GasType> GasTypes { get; set; }
    public DbSet<HeatingType> HeatingTypes { get; set; }
    public DbSet<LandCategory> LandCategories { get; set; }
    public DbSet<LayoutType> LayoutTypes { get; set; }
    public DbSet<OfficeClass> OfficeClass { get; set; }
    public DbSet<RenovationType> RenovationTypes { get; set; }
    public DbSet<RoadAccessType> RoadAccessTypes { get; set; }
    public DbSet<SewerType> SewerTypes { get; set; }
    public DbSet<WallMaterial> WallMaterials { get; set; }
    public DbSet<WaterSupplyType> WaterSupplyTypes { get; set; }

#endregion

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;
using Microsoft.AspNetCore.Builder;

namespace ListingService.Infrastructure.Data;

public static class DatabaseInitialaizer
{
    public static void InitializeTestDatabase(this WebApplication application,
        bool ensureDelete = false)
    {
        var scope = application.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();

        initializer.Initialize(ensureDelete);
    }

    public static async Task InitializeTestDatabaseAsync(this WebApplication application,
        bool ensureDelete = false)
    {
        using var scope = application.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();

        await initializer.InitializeAsync(ensureDelete);
    }

    public static DbContextOptionsBuilder UseSeedingAsync(this DbContextOptionsBuilder builder, IServiceProvider provider)
    {
        builder.UseAsyncSeeding(async (context, _, ct) =>
        {
            var initializer = provider.GetRequiredService<ApplicationDatabaseInitializer>();
        
            await initializer.SeedAsync();
        });

        return builder;
    }

    public static DbContextOptionsBuilder UseSeeding(this DbContextOptionsBuilder builder, IServiceProvider provider)
    {
        builder.UseSeeding((context, _) =>
        {
            var initializer = provider.GetRequiredService<ApplicationDatabaseInitializer>();

            initializer.Seed();
        });

        return builder;
    }
}

public class ApplicationDatabaseInitializer(ApplicationDbContext context)
{
    public void Initialize(bool ensureDelete = false)
    {
        try
        {
            if (ensureDelete)
            {
                context.Database.EnsureDeleted();
            }
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        }
    }

    public async Task InitializeAsync(bool ensureDelete = false)
    {
        try
        {
            if (ensureDelete)
            {
                await context.Database.EnsureDeletedAsync();
            }
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        }
    }

#region Seed data

    static readonly List<ListingType> listingTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000020001"), Name = "Sale" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000020002"), Name = "Rent" }
    ];

    static readonly List<BathroomType> bathroomTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030001"), Name = "Combined (shared)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030002"), Name = "Separate" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030003"), Name = "Two or more" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030004"), Name = "Bathroom + WC (separate rooms)" }
    ];

    static readonly List<RenovationType> renovationTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040001"), Name = "No renovation / requires renovation" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040002"), Name = "Cosmetic" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040003"), Name = "Major / Capital" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040004"), Name = "Designer" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040005"), Name = "Shell & Core" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040006"), Name = "Finished / Turnkey" }
    ];

    static readonly List<BuildingMaterial> buildingMaterials =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050001"), Name = "Brick" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050002"), Name = "Panel" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050003"), Name = "Monolith" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050004"), Name = "Monolith-Brick" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050005"), Name = "Wood" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050006"), Name = "Block (foam/gas block)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050007"), Name = "Metal frame" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050008"), Name = "Other" }
    ];

    static readonly List<WallMaterial> wallMaterials =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060001"), Name = "Brick" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060002"), Name = "Concrete" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060003"), Name = "Sandwich panels" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060004"), Name = "Metal" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060005"), Name = "Wood" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060006"), Name = "Block (foam/gas block)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060007"), Name = "Other" }
    ];

    static readonly List<HeatingType> heatingTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070001"), Name = "Central" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070002"), Name = "Autonomous (building)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070003"), Name = "Individual gas boiler" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070004"), Name = "Electric" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070005"), Name = "Solid fuel" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070006"), Name = "None" }
    ];

    static readonly List<WaterSupplyType> waterSupplyTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080001"), Name = "Central" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080002"), Name = "Well" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080003"), Name = "Borehole" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080004"), Name = "Delivered / tank" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080005"), Name = "None" }
    ];

    static readonly List<SewerType> sewerTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090001"), Name = "Central" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090002"), Name = "Septic tank" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090003"), Name = "Cesspool" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090004"), Name = "None" }
    ];

    static readonly List<GasType> gasTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0001"), Name = "Central gas" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0002"), Name = "Gas holder / tank" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0003"), Name = "Bottled gas" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0004"), Name = "None" }
    ];

    static readonly List<LandCategory> landCategories =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0001"), Name = "Residential (IЖС)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0002"), Name = "Personal subsidiary plot (ЛПХ)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0003"), Name = "Garden / dacha (СНТ)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0004"), Name = "Agricultural" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0005"), Name = "Industrial / commercial" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0006"), Name = "Forestry / protected" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0007"), Name = "Other" }
    ];

    static readonly List<RoadAccessType> roadAccessTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0001"), Name = "Asphalt" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0002"), Name = "Gravel" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0003"), Name = "Dirt road" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0004"), Name = "No direct access / off-road" }
    ];

    static readonly List<ClassType> classTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0001"), Name = "A" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0002"), Name = "B" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0003"), Name = "C" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0004"), Name = "D" }
    ];

    static readonly List<LayoutType> layoutTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0001"), Name = "Open space" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0002"), Name = "Cabinet / rooms" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0003"), Name = "Mixed" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0004"), Name = "Coworking" }
    ];

    static readonly List<OfficeClass> officeClasses =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0001"), Name = "A" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0002"), Name = "B+" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0003"), Name = "B" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0004"), Name = "C" }
    ];

    static readonly List<BuildingType> buildingTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010001"), Name = "Apartment Building (Multi-family)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010002"), Name = "Detached House" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010003"), Name = "Townhouse" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010004"), Name = "Business Center" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010005"), Name = "Warehouse Complex" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010006"), Name = "Industrial Building" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010007"), Name = "Shopping Mall" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010008"), Name = "Mixed-use Building" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010009"), Name = "Other" }
    ];

#endregion

    public void Seed()
    {
        try
        {
            if (!context.ListingTypes.Any())
            {
                context.ListingTypes.AddRange(listingTypes);
            }

            if (!context.BathroomTypes.Any())
            {
                context.BathroomTypes.AddRange(bathroomTypes);
            }

            if (!context.RenovationTypes.Any())
            {
                context.RenovationTypes.AddRange(renovationTypes);
            }

            if (!context.BuildingMaterials.Any())
            {
                context.BuildingMaterials.AddRange(buildingMaterials);
            }

            if (!context.WallMaterials.Any())
            {
                context.WallMaterials.AddRange(wallMaterials);
            }

            if (!context.HeatingTypes.Any())
            {
                context.HeatingTypes.AddRange(heatingTypes);
            }

            if (!context.WaterSupplyTypes.Any())
            {
                context.WaterSupplyTypes.AddRange(waterSupplyTypes);
            }

            if (!context.SewerTypes.Any())
            {
                context.SewerTypes.AddRange(sewerTypes);
            }

            if (!context.GasTypes.Any())
            {
                context.GasTypes.AddRange(gasTypes);
            }

            if (!context.LandCategories.Any())
            {
                context.LandCategories.AddRange(landCategories);
            }

            if (!context.RoadAccessTypes.Any())
            {
                context.RoadAccessTypes.AddRange(roadAccessTypes);
            }

            if (!context.ClassTypes.Any())
            {
                context.ClassTypes.AddRange(classTypes);
            }

            if (!context.LayoutTypes.Any())
            {
                context.LayoutTypes.AddRange(layoutTypes);
            }

            if (!context.OfficeClass.Any())
            {
                context.OfficeClass.AddRange(officeClasses);
            }

            if (!context.BuildingTypes.Any())
            {
                context.BuildingTypes.AddRange(buildingTypes);
            }

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        }
    }

    //TODO reduce by reflection
    public async Task SeedAsync()
    {
        try
        {
            if (!await context.ListingTypes.AnyAsync())
            {
                await context.ListingTypes.AddRangeAsync(listingTypes);
            }

            if (!await context.BathroomTypes.AnyAsync())
            {
                await context.BathroomTypes.AddRangeAsync(bathroomTypes);
            }

            if (!await context.RenovationTypes.AnyAsync())
            {
                await context.RenovationTypes.AddRangeAsync(renovationTypes);
            }

            if (!await context.BuildingMaterials.AnyAsync())
            {
                await context.BuildingMaterials.AddRangeAsync(buildingMaterials);
            }

            if (!await context.WallMaterials.AnyAsync())
            {
                await context.WallMaterials.AddRangeAsync(wallMaterials);
            }

            if (!await context.HeatingTypes.AnyAsync())
            {
                await context.HeatingTypes.AddRangeAsync(heatingTypes);
            }

            if (!await context.WaterSupplyTypes.AnyAsync())
            {
                await context.WaterSupplyTypes.AddRangeAsync(waterSupplyTypes);
            }

            if (!await context.SewerTypes.AnyAsync())
            {
                await context.SewerTypes.AddRangeAsync(sewerTypes);
            }

            if (!await context.GasTypes.AnyAsync())
            {
                await context.GasTypes.AddRangeAsync(gasTypes);
            }

            if (!await context.LandCategories.AnyAsync())
            {
                await context.LandCategories.AddRangeAsync(landCategories);
            }

            if (!await context.RoadAccessTypes.AnyAsync())
            {
                await context.RoadAccessTypes.AddRangeAsync(roadAccessTypes);
            }

            if (!await context.ClassTypes.AnyAsync())
            {
                await context.ClassTypes.AddRangeAsync(classTypes);
            }

            if (!await context.LayoutTypes.AnyAsync())
            {
                await context.LayoutTypes.AddRangeAsync(layoutTypes);
            }

            if (!await context.OfficeClass.AnyAsync())
            {
                await context.OfficeClass.AddRangeAsync(officeClasses);
            }

            if (!await context.BuildingTypes.AnyAsync())
            {
                await context.BuildingTypes.AddRangeAsync(buildingTypes);
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        }
    }
}
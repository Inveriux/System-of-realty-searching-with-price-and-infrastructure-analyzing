using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;
using Microsoft.AspNetCore.Builder;

namespace ListingService.Infrastructure.Data;

public static class DatabaseInitialaizer
{
    public static void InitializeDatabase(this WebApplication application)
    {
        var scope = application.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();

        initializer.Initialize();
    }

    public static async Task InitializeDatabaseAsync(this WebApplication application)
    {
        using var scope = application.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();

        await initializer.InitializeAsync();
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
    public void Initialize()
    {
        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
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
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000020001"), TypeName = "Sale" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000020002"), TypeName = "Rent" }
    ];

    static readonly List<BathroomType> bathroomTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030001"), TypeName = "Combined (shared)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030002"), TypeName = "Separate" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030003"), TypeName = "Two or more" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030004"), TypeName = "Bathroom + WC (separate rooms)" }
    ];

    static readonly List<RenovationType> renovationTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040001"), TypeName = "No renovation / requires renovation" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040002"), TypeName = "Cosmetic" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040003"), TypeName = "Major / Capital" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040004"), TypeName = "Designer" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040005"), TypeName = "Shell & Core" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040006"), TypeName = "Finished / Turnkey" }
    ];

    static readonly List<BuildingMaterial> buildingMaterials =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050001"), TypeName = "Brick" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050002"), TypeName = "Panel" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050003"), TypeName = "Monolith" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050004"), TypeName = "Monolith-Brick" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050005"), TypeName = "Wood" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050006"), TypeName = "Block (foam/gas block)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050007"), TypeName = "Metal frame" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050008"), TypeName = "Other" }
    ];

    static readonly List<WallMaterial> wallMaterials =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060001"), TypeName = "Brick" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060002"), TypeName = "Concrete" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060003"), TypeName = "Sandwich panels" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060004"), TypeName = "Metal" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060005"), TypeName = "Wood" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060006"), TypeName = "Block (foam/gas block)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060007"), TypeName = "Other" }
    ];

    static readonly List<HeatingType> heatingTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070001"), TypeName = "Central" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070002"), TypeName = "Autonomous (building)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070003"), TypeName = "Individual gas boiler" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070004"), TypeName = "Electric" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070005"), TypeName = "Solid fuel" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070006"), TypeName = "None" }
    ];

    static readonly List<WaterSupplyType> waterSupplyTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080001"), TypeName = "Central" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080002"), TypeName = "Well" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080003"), TypeName = "Borehole" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080004"), TypeName = "Delivered / tank" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080005"), TypeName = "None" }
    ];

    static readonly List<SewerType> sewerTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090001"), TypeName = "Central" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090002"), TypeName = "Septic tank" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090003"), TypeName = "Cesspool" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090004"), TypeName = "None" }
    ];

    static readonly List<GasType> gasTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0001"), TypeName = "Central gas" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0002"), TypeName = "Gas holder / tank" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0003"), TypeName = "Bottled gas" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0004"), TypeName = "None" }
    ];

    static readonly List<LandCategory> landCategories =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0001"), TypeName = "Residential (IЖС)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0002"), TypeName = "Personal subsidiary plot (ЛПХ)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0003"), TypeName = "Garden / dacha (СНТ)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0004"), TypeName = "Agricultural" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0005"), TypeName = "Industrial / commercial" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0006"), TypeName = "Forestry / protected" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0007"), TypeName = "Other" }
    ];

    static readonly List<RoadAccessType> roadAccessTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0001"), TypeName = "Asphalt" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0002"), TypeName = "Gravel" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0003"), TypeName = "Dirt road" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0004"), TypeName = "No direct access / off-road" }
    ];

    static readonly List<ClassType> classTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0001"), TypeName = "A" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0002"), TypeName = "B" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0003"), TypeName = "C" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000D0004"), TypeName = "D" }
    ];

    static readonly List<LayoutType> layoutTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0001"), ClassName = "Open space" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0002"), ClassName = "Cabinet / rooms" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0003"), ClassName = "Mixed" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0004"), ClassName = "Coworking" }
    ];

    static readonly List<OfficeClass> officeClasses =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0001"), ClassName = "A" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0002"), ClassName = "B+" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0003"), ClassName = "B" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000F0004"), ClassName = "C" }
    ];

    static readonly List<BuildingType> buildingTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010001"), TypeName = "Apartment Building (Multi-family)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010002"), TypeName = "Detached House" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010003"), TypeName = "Townhouse" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010004"), TypeName = "Business Center" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010005"), TypeName = "Warehouse Complex" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010006"), TypeName = "Industrial Building" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010007"), TypeName = "Shopping Mall" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010008"), TypeName = "Mixed-use Building" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010009"), TypeName = "Other" }
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
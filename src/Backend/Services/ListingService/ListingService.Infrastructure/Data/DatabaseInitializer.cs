using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;
using ListingService.Domain.Abstractions;
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
            Seed();
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
            await SeedAsync();
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
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0001"), Name = "Residential development" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0002"), Name = "Personal subsidiary farming" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0003"), Name = "Garden / dacha plot" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0004"), Name = "Agricultural" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0005"), Name = "Industrial / commercial" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0006"), Name = "Forestry / protected area" },
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
            SeedLookup(context.ListingTypes, listingTypes);
            SeedLookup(context.BathroomTypes, bathroomTypes);
            SeedLookup(context.RenovationTypes, renovationTypes);
            SeedLookup(context.BuildingMaterials, buildingMaterials);
            SeedLookup(context.WallMaterials, wallMaterials);
            SeedLookup(context.HeatingTypes, heatingTypes);
            SeedLookup(context.WaterSupplyTypes, waterSupplyTypes);
            SeedLookup(context.SewerTypes, sewerTypes);
            SeedLookup(context.GasTypes, gasTypes);
            SeedLookup(context.LandCategories, landCategories);
            SeedLookup(context.RoadAccessTypes, roadAccessTypes);
            SeedLookup(context.ClassTypes, classTypes);
            SeedLookup(context.LayoutTypes, layoutTypes);
            SeedLookup(context.OfficeClass, officeClasses);
            SeedLookup(context.BuildingTypes, buildingTypes);

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
            await SeedLookupAsync(context.ListingTypes, listingTypes);
            await SeedLookupAsync(context.BathroomTypes, bathroomTypes);
            await SeedLookupAsync(context.RenovationTypes, renovationTypes);
            await SeedLookupAsync(context.BuildingMaterials, buildingMaterials);
            await SeedLookupAsync(context.WallMaterials, wallMaterials);
            await SeedLookupAsync(context.HeatingTypes, heatingTypes);
            await SeedLookupAsync(context.WaterSupplyTypes, waterSupplyTypes);
            await SeedLookupAsync(context.SewerTypes, sewerTypes);
            await SeedLookupAsync(context.GasTypes, gasTypes);
            await SeedLookupAsync(context.LandCategories, landCategories);
            await SeedLookupAsync(context.RoadAccessTypes, roadAccessTypes);
            await SeedLookupAsync(context.ClassTypes, classTypes);
            await SeedLookupAsync(context.LayoutTypes, layoutTypes);
            await SeedLookupAsync(context.OfficeClass, officeClasses);
            await SeedLookupAsync(context.BuildingTypes, buildingTypes);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        }
    }

    private static void SeedLookup<TLookup>(DbSet<TLookup> dbSet, IReadOnlyCollection<TLookup> seedItems)
        where TLookup : class, ILookupEntity
    {
        var existingById = dbSet.ToDictionary(item => item.Id);

        foreach (var seedItem in seedItems)
        {
            if (existingById.TryGetValue(seedItem.Id, out var existing))
            {
                if (!string.Equals(existing.Name, seedItem.Name, StringComparison.Ordinal))
                    existing.Name = seedItem.Name;

                continue;
            }

            dbSet.Add(seedItem);
        }
    }

    private static async Task SeedLookupAsync<TLookup>(DbSet<TLookup> dbSet, IReadOnlyCollection<TLookup> seedItems)
        where TLookup : class, ILookupEntity
    {
        var existingById = await dbSet.ToDictionaryAsync(item => item.Id);

        foreach (var seedItem in seedItems)
        {
            if (existingById.TryGetValue(seedItem.Id, out var existing))
            {
                if (!string.Equals(existing.Name, seedItem.Name, StringComparison.Ordinal))
                    existing.Name = seedItem.Name;

                continue;
            }

            await dbSet.AddAsync(seedItem);
        }
    }
}

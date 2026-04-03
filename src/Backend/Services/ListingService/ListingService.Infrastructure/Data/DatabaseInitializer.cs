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
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000020001"), Name = "Продажа" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000020002"), Name = "Аренда" }
    ];

    static readonly List<BathroomType> bathroomTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030001"), Name = "Совмещенный" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030002"), Name = "Раздельный" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030003"), Name = "Два и более" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000030004"), Name = "Ванная и туалет раздельно" }
    ];

    static readonly List<RenovationType> renovationTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040001"), Name = "Без ремонта / требует ремонта" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040002"), Name = "Косметический" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040003"), Name = "Капитальный" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040004"), Name = "Дизайнерский" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040005"), Name = "Под чистовую отделку" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000040006"), Name = "С отделкой / под ключ" }
    ];

    static readonly List<BuildingMaterial> buildingMaterials =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050001"), Name = "Кирпич" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050002"), Name = "Панель" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050003"), Name = "Монолит" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050004"), Name = "Монолит-кирпич" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050005"), Name = "Дерево" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050006"), Name = "Блоки (пеноблок / газоблок)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050007"), Name = "Металлокаркас" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000050008"), Name = "Другое" }
    ];

    static readonly List<WallMaterial> wallMaterials =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060001"), Name = "Кирпич" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060002"), Name = "Бетон" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060003"), Name = "Сэндвич-панели" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060004"), Name = "Металл" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060005"), Name = "Дерево" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060006"), Name = "Блоки (пеноблок / газоблок)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000060007"), Name = "Другое" }
    ];

    static readonly List<HeatingType> heatingTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070001"), Name = "Центральное" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070002"), Name = "Автономное (на здание)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070003"), Name = "Индивидуальный газовый котел" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070004"), Name = "Электрическое" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070005"), Name = "Твердотопливное" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000070006"), Name = "Нет" }
    ];

    static readonly List<WaterSupplyType> waterSupplyTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080001"), Name = "Центральное" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080002"), Name = "Колодец" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080003"), Name = "Скважина" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080004"), Name = "Привозная вода / емкость" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000080005"), Name = "Нет" }
    ];

    static readonly List<SewerType> sewerTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090001"), Name = "Центральная" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090002"), Name = "Септик" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090003"), Name = "Выгребная яма" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000090004"), Name = "Нет" }
    ];

    static readonly List<GasType> gasTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0001"), Name = "Центральный газ" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0002"), Name = "Газгольдер / резервуар" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0003"), Name = "Баллонный газ" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000A0004"), Name = "Нет" }
    ];

    static readonly List<LandCategory> landCategories =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0001"), Name = "Жилая застройка (ИЖС)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0002"), Name = "Личное подсобное хозяйство (ЛПХ)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0003"), Name = "Садовый / дачный участок (СНТ)" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0004"), Name = "Сельскохозяйственная" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0005"), Name = "Промышленная / коммерческая" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0006"), Name = "Лесной фонд / охраняемая территория" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000B0007"), Name = "Другое" }
    ];

    static readonly List<RoadAccessType> roadAccessTypes =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0001"), Name = "Асфальт" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0002"), Name = "Гравий" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0003"), Name = "Грунтовая дорога" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000C0004"), Name = "Нет прямого подъезда / бездорожье" }
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
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0001"), Name = "Открытая планировка" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0002"), Name = "Кабинеты / комнаты" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0003"), Name = "Смешанная" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000E0004"), Name = "Коворкинг" }
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
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010001"), Name = "Многоквартирный дом" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010002"), Name = "Отдельный дом" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010003"), Name = "Таунхаус" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010004"), Name = "Бизнес-центр" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010005"), Name = "Складской комплекс" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010006"), Name = "Промышленное здание" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010007"), Name = "Торговый центр" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010008"), Name = "Многофункциональное здание" },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000010009"), Name = "Другое" }
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

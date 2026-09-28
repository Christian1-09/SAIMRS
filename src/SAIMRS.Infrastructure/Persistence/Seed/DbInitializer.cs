using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Infrastructure.Persistence.Seed;

/// <summary>
/// Runs once at startup: creates/updates the database schema and inserts the
/// default settings and categories. Sample data is optional (for demos and testing).
/// The admin account is NOT seeded — it is created on the first-run Setup screen.
/// </summary>
public sealed class DbInitializer(AppDbContext db, IDateTimeProvider clock)
{
    private const string SampleUser = "System (sample data)";

    public async Task InitializeAsync(bool seedSampleData, CancellationToken ct = default)
    {
        // Use migrations when they exist (recommended, see README). Without migrations the
        // schema is created directly, which is fine for a first test run.
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync(ct);
        else
            await db.Database.EnsureCreatedAsync(ct);

        var now = clock.Now;

        if (!await db.SystemSettings.AnyAsync(ct))
            db.SystemSettings.Add(SystemSettings.CreateDefault(now));

        if (!await db.Categories.AnyAsync(ct))
        {
            db.Categories.AddRange(
                new Category("Seeds", "Planting seeds for various crops", now),
                new Category("Fertilizers", "Chemical and organic fertilizers", now),
                new Category("Pesticides", "Insecticides, fungicides, herbicides", now),
                new Category("Harvests", "Harvested crop outputs", now),
                new Category("Tools & Equipment", "Farm tools and machinery", now));
        }

        await db.SaveChangesAsync(ct);

        if (seedSampleData && !await db.InventoryItems.AnyAsync(ct))
            await SeedSampleDataAsync(now, ct);
    }

    /// <summary>Sample records based on the original prototype, dated relative to today.</summary>
    private async Task SeedSampleDataAsync(DateTime now, CancellationToken ct)
    {
        var categories = await db.Categories.ToDictionaryAsync(c => c.Name, ct);
        var seeds = categories["Seeds"];
        var fertilizers = categories["Fertilizers"];
        var pesticides = categories["Pesticides"];
        var harvests = categories["Harvests"];
        var tools = categories["Tools & Equipment"];

        // Suppliers
        var agriSource = new Supplier("AgriSource PH", "Ricardo Lim", "0917-555-1234", "Seeds, Tools", now);
        var zambAgri = new Supplier("Zamboanga Agri Supply", "Fatima Musa", "0918-444-5678", "Pesticides", now);
        var philSeeds = new Supplier("PhilSeeds Corp", "Jose Bautista", "0916-222-9012", "Rice & Corn Seeds", now);
        var greenEarth = new Supplier("Green Earth Farm Supplies", "Ana Cruz", "0919-333-3456", "Fertilizers", now);
        db.Suppliers.AddRange(agriSource, zambAgri, philSeeds, greenEarth);

        // Beneficiaries
        var today = DateOnly.FromDateTime(now);
        var juan = new Beneficiary("BEN-0001", "Juan Dela Cruz", "Upper Irasan", "0917-123-4567", 1.5m, CropType.Rice, BeneficiaryStatus.Active, today.AddMonths(-8), now);
        var maria = new Beneficiary("BEN-0002", "Maria Santos", "Lower Irasan", "0918-234-5678", 2.0m, CropType.Corn, BeneficiaryStatus.Active, today.AddMonths(-7), now);
        var pedro = new Beneficiary("BEN-0003", "Pedro Reyes", "Manicahan", "0919-345-6789", 0.8m, CropType.Vegetables, BeneficiaryStatus.Active, today.AddMonths(-7), now);
        var paicFarm = new Beneficiary("BEN-0004", "PAIC Farm", "Upper Irasan", "0920-456-7890", 5.0m, CropType.Mixed, BeneficiaryStatus.Active, today.AddMonths(-9), now);
        var rosario = new Beneficiary("BEN-0005", "Rosario Villanueva", "Baliwasan", "0921-567-8901", 1.2m, CropType.Rice, BeneficiaryStatus.Inactive, today.AddMonths(-10), now);
        db.Beneficiaries.AddRange(juan, maria, pedro, paicFarm, rosario);

        // Items, each with an opening balance and a few movements spread over the last months
        DateTime Ago(int days, int hour = 9) => now.Date.AddDays(-days).AddHours(hour);

        var riceSeeds = NewItem("Rice Seeds (NSIC Rc222)", seeds, philSeeds, "kg", 20, 85);
        riceSeeds.Receive(60, 85, philSeeds, "DR-SAMPLE-001", "Opening balance", SampleUser, Ago(160));
        riceSeeds.Issue(20, juan, "ISS-SAMPLE-001", "Planting season distribution", SampleUser, Ago(120));
        riceSeeds.Issue(15, paicFarm, "ISS-SAMPLE-002", null, SampleUser, Ago(75));
        riceSeeds.Issue(13, juan, "ISS-SAMPLE-003", null, SampleUser, Ago(10));

        var cornSeeds = NewItem("Corn Seeds (OPV)", seeds, agriSource, "kg", 15, 60);
        cornSeeds.Receive(40, 60, agriSource, "DR-SAMPLE-002", "Opening balance", SampleUser, Ago(150));
        cornSeeds.Receive(60, 62, agriSource, "DR-SAMPLE-041", "Received from AgriSource PH", SampleUser, Ago(40));
        cornSeeds.Issue(50, maria, "ISS-SAMPLE-004", null, SampleUser, Ago(35));

        var urea = NewItem("Urea Fertilizer (46-0-0)", fertilizers, greenEarth, "bag", 10, 1250);
        urea.Receive(20, 1250, greenEarth, "DR-SAMPLE-003", "Opening balance", SampleUser, Ago(140));
        urea.Issue(10, paicFarm, "ISS-SAMPLE-005", null, SampleUser, Ago(90));
        urea.AdjustToCount(8, "ADJ-SAMPLE-001", "Damaged bags removed", SampleUser, Ago(20, 14));

        var complete = NewItem("Complete Fertilizer (14-14-14)", fertilizers, greenEarth, "bag", 10, 1100);
        complete.Receive(43, 1100, greenEarth, "DR-SAMPLE-004", "Opening balance", SampleUser, Ago(130));
        complete.Issue(8, paicFarm, "ISS-SAMPLE-006", null, SampleUser, Ago(3));

        var malathion = NewItem("Malathion Pesticide", pesticides, zambAgri, "liter", 5, 320);
        malathion.Receive(30, 320, zambAgri, "DR-SAMPLE-005", "Opening balance", SampleUser, Ago(110));
        malathion.Issue(8, pedro, "ISS-SAMPLE-007", null, SampleUser, Ago(60));

        var roundup = NewItem("Roundup Herbicide", pesticides, zambAgri, "liter", 5, 450);
        roundup.Receive(10, 450, zambAgri, "DR-SAMPLE-006", "Opening balance", SampleUser, Ago(100));
        roundup.Issue(7, maria, "ISS-SAMPLE-008", null, SampleUser, Ago(25));

        var corn = NewItem("Harvested Corn (Batch 47)", harvests, null, "kg", 0, null);
        corn.Receive(220, null, null, "HR-SAMPLE-047", "Harvest delivery from upper farm", SampleUser, Ago(0, 7));

        var sprayer = NewItem("Hand Sprayer 16L", tools, agriSource, "piece", 2, 850);
        sprayer.Receive(4, 850, agriSource, "DR-SAMPLE-007", "Opening balance", SampleUser, Ago(170));

        db.InventoryItems.AddRange(riceSeeds, cornSeeds, urea, complete, malathion, roundup, corn, sprayer);

        // Perishable batches
        db.PerishableBatches.AddRange(
            new PerishableBatch("Urea Fertilizer (46-0-0)", "BATCH-021", 8, "bag", today.AddMonths(-8), today.AddDays(5), null, now),
            new PerishableBatch("Malathion Pesticide", "BATCH-018", 5, "liter", today.AddMonths(-10), today.AddDays(9), null, now),
            new PerishableBatch("Harvested Corn (Batch 47)", "BATCH-047", 220, "kg", today, today.AddDays(28), null, now),
            new PerishableBatch("Rice Seeds (NSIC Rc222)", "BATCH-033", 20, "kg", today.AddMonths(-7), today.AddMonths(3), null, now));

        await db.SaveChangesAsync(ct);

        InventoryItem NewItem(string name, Category category, Supplier? supplier, string unit, decimal threshold, decimal? cost) =>
            new(name, category, supplier, unit, threshold, cost, null, Ago(180));
    }
}

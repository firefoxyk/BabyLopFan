using GuildApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GuildApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<HuntCalculation> HuntCalculations => Set<HuntCalculation>();
    public DbSet<WorldFameCalculation> WorldFameCalculations => Set<WorldFameCalculation>();
    public DbSet<ClosenessCalculation> ClosenessCalculations => Set<ClosenessCalculation>();
    public DbSet<AssistantCalculation> AssistantCalculations => Set<AssistantCalculation>();
    public DbSet<PowerCalculation> PowerCalculations => Set<PowerCalculation>();
    public DbSet<BanquetCalculation> BanquetCalculations => Set<BanquetCalculation>();
}

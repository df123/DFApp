using DFApp.IP;
using DFApp.Lottery;
using DFApp.Media;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.CmsKit.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using DFApp.DataFilters;
using System;
using Volo.Abp.Users;
using DFApp.Bookkeeping;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using DFApp.Configuration;

namespace DFApp.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class DFAppDbContext :
    AbpDbContext<DFAppDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    protected bool CreatorIdFilterEnabled => DataFilter?.IsEnabled<ICreatorId>() ?? false;

    private ICurrentUser _currentUser => LazyServiceProvider.LazyGetRequiredService<ICurrentUser>();

    private Guid? _currentUserId => _currentUser?.Id;

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DFAppDbContext(DbContextOptions<DFAppDbContext> options)
        : base(options)
    {

    }

    public DbSet<MediaInfo> MediaInfos { get; set; }
    public DbSet<DynamicIP> DynamicIPs { get; set; }

    public DbSet<LotteryInfo> LotteryInfos { get; set; }

    public DbSet<LotteryResult> LotteryResults { get; set; }
    public DbSet<LotteryPrizegrades> LotteryPrizegrades { get; set; }

    public DbSet<BookkeepingCategory> BookkeepingCategories { get; set; }
    public DbSet<BookkeepingExpenditure> bookkeepingExpenditures { get; set; }

    public DbSet<ConfigurationInfo> ConfigurationInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        builder.Entity<MediaInfo>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "MediaInfo", DFAppConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DynamicIP>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "DynamicIP", DFAppConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<LotteryInfo>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "Lottery", DFAppConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<LotteryResult>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "LotteryResult", DFAppConsts.DbSchema);
            b.HasMany(e => e.Prizegrades)
            .WithOne(e => e.Result)
            .HasForeignKey(e => e.LotteryResultId);
            b.ConfigureByConvention();

            b.HasIndex(e => new { e.Code, e.Name })
            .IsUnique();
        });

        builder.Entity<LotteryPrizegrades>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "LotteryPrizegrades", DFAppConsts.DbSchema);
            b.ConfigureByConvention();
        });
        builder.ConfigureCmsKit();
        builder.ConfigureBlobStoring();


        builder.Entity<BookkeepingExpenditure>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "BookkeepingExpenditure", DFAppConsts.DbSchema);

            b.ConfigureByConvention();
        });

        builder.Entity<BookkeepingCategory>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "BookkeepingCategory", DFAppConsts.DbSchema);

            b.HasMany(e => e.Expenditures)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId);

            b.HasIndex(e => new { e.Category, e.CreatorId })
            .IsUnique();

            b.ConfigureByConvention();
        });

        builder.Entity<ConfigurationInfo>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "ConfigurationInfo", DFAppConsts.DbSchema);

            b.HasIndex(e => new { e.ModuleName, e.ConfigurationName })
            .IsUnique();

            b.ConfigureByConvention();
        });

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder.LogTo(System.Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging();
#endif
        base.OnConfiguring(optionsBuilder);
    }


    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(ICreatorId).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>()
    {
        var expression = base.CreateFilterExpression<TEntity>();

        if (typeof(ICreatorId).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> creatorIdFilter =
                e => !CreatorIdFilterEnabled
                || (_currentUserId != null && (EF.Property<Guid>(e, "CreatorId") == _currentUserId));

            expression = expression == null
                ? creatorIdFilter
                : QueryFilterExpressionHelper.CombineExpressions(expression, creatorIdFilter);
        }

        return expression;

    }


}

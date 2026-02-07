using DFApp.IP;
using DFApp.Lottery;
using DFApp.Media;
using DFApp.Rss;
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
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using DFApp.DataFilters;
using System;
using Volo.Abp.Users;
using DFApp.Bookkeeping;
using Microsoft.EntityFrameworkCore.Metadata;
 using System.Linq.Expressions;
using DFApp.FileUploadDownload;
using DFApp.Configuration;
using DFApp.Aria2.Response.TellStatus;
 using DFApp.FileFilter;

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
    public DbSet<IdentitySession> Sessions { get; set; }

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
    public DbSet<FileUploadInfo> FileUploadInfos { get; set; }

    public DbSet<MediaExternalLink> MediaExternalLinks { get; set; }
    public DbSet<MediaExternalLinkMediaIds> ExternalLinkMediaIds { get; set; }

    public DbSet<TellStatusResult> TellStatusResults { get; set; }
    public DbSet<FilesItem> FilesItems { get; set; }
    public DbSet<UrisItem> UrisItems { get; set; }
    public DbSet<LotterySimulation> LotterySimulations { get; set; }
    public DbSet<KeywordFilterRule> KeywordFilterRules { get; set; }
     public DbSet<RssSource> RssSources { get; set; }
     public DbSet<RssMirrorItem> RssMirrorItems { get; set; }
     public DbSet<RssWordSegment> RssWordSegments { get; set; }
      
      public DbSet<DFApp.ElectricVehicle.ElectricVehicle> ElectricVehicles { get; set; }
      public DbSet<DFApp.ElectricVehicle.ElectricVehicleCost> ElectricVehicleCosts { get; set; }
      public DbSet<DFApp.ElectricVehicle.ElectricVehicleChargingRecord> ElectricVehicleChargingRecords { get; set; }
      public DbSet<DFApp.ElectricVehicle.GasolinePrice> GasolinePrices { get; set; }

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
            b.HasIndex(e => e.MediaId);
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
        builder.ConfigureBlobStoring();


        builder.Entity<BookkeepingExpenditure>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "BookkeepingExpenditure", DFAppConsts.DbSchema);
            b.Property(e => e.IsBelongToSelf)
            .HasDefaultValue(true);
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

        builder.Entity<FileUploadInfo>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "FileUploadInfo", DFAppConsts.DbSchema);

            b.HasIndex(e => e.Sha1)
            .IsUnique();

            b.ConfigureByConvention();
        });

        builder.Entity<MediaExternalLink>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "MediaExternalLink", DFAppConsts.DbSchema);
            
            b.HasMany(e => e.MediaIds)
            .WithOne(e => e.ExternalLink)
            .HasForeignKey(e => e.MediaExternalLinkId)
            .IsRequired();

            b.ConfigureByConvention();
        });

        builder.Entity<MediaExternalLinkMediaIds>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "MediaExternalLinkMediaIds", DFAppConsts.DbSchema);

            b.ConfigureByConvention();
        });

        builder.Entity<TellStatusResult>(b =>
        {
            b.ToTable(DFAppConsts.DbTableAria2Prefix + "TellStatusResult", DFAppConsts.DbSchema);

            b.ConfigureByConvention();
        });

        builder.Entity<FilesItem>(b =>
        {
            b.ToTable(DFAppConsts.DbTableAria2Prefix + "FilesItem", DFAppConsts.DbSchema);

            b.HasOne(e => e.Result)
            .WithMany(e => e.Files)
            .HasForeignKey(e => e.ResultId);

            b.ConfigureByConvention();
        });

        builder.Entity<UrisItem>(b =>
        {
            b.ToTable(DFAppConsts.DbTableAria2Prefix + "UrisItem", DFAppConsts.DbSchema);

            b.HasOne(e => e.FilesItem)
            .WithMany(e => e.Uris)
            .HasForeignKey(e => e.FilesItemId);

            b.ConfigureByConvention();
        });

        builder.Entity<LotterySimulation>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "LotterySimulation", DFAppConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(e => new { e.TermNumber, e.GroupId });
        });

        builder.Entity<KeywordFilterRule>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "KeywordFilterRule", DFAppConsts.DbSchema);
            b.ConfigureByConvention();

            // 添加索引以提高查询性能
            b.HasIndex(e => e.IsEnabled);
            b.HasIndex(e => e.FilterType);
            b.HasIndex(e => e.Priority);
            b.HasIndex(e => new { e.IsEnabled, e.Priority });
        });

        builder.Entity<RssMirrorItem>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "RssMirrorItem", DFAppConsts.DbSchema);
            b.ConfigureByConvention();

            // 添加索引以提高查询性能
            b.HasIndex(e => e.RssSourceId);
            b.HasIndex(e => e.PublishDate);
            b.HasIndex(e => e.CreationTime);
            b.HasIndex(e => e.IsDownloaded);
        });

        builder.Entity<RssWordSegment>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "RssWordSegment", DFAppConsts.DbSchema);
            b.ConfigureByConvention();

            // 添加索引以提高查询性能
            b.HasIndex(e => e.RssMirrorItemId);
            b.HasIndex(e => e.Word);
            b.HasIndex(e => e.LanguageType);
            b.HasIndex(e => e.Count);
        });

        builder.Entity<RssSource>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "RssSource", DFAppConsts.DbSchema);
            b.ConfigureByConvention();
 
            // 添加索引以提高查询性能
            b.HasIndex(e => e.IsEnabled);
            b.HasIndex(e => e.FetchStatus);
        });

        builder.Entity<DFApp.ElectricVehicle.ElectricVehicle>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "ElectricVehicle", DFAppConsts.DbSchema);
            b.Property(e => e.TotalMileage)
            .HasDefaultValue(0);
            b.ConfigureByConvention();
        });

        builder.Entity<DFApp.ElectricVehicle.ElectricVehicleCost>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "ElectricVehicleCost", DFAppConsts.DbSchema);
            b.Property(e => e.IsBelongToSelf)
            .HasDefaultValue(true);

            b.HasOne(e => e.Vehicle)
            .WithMany(e => e.Costs)
            .HasForeignKey(e => e.VehicleId);

            b.HasIndex(e => e.CostDate);
            b.HasIndex(e => e.CostType);
            b.HasIndex(e => e.VehicleId);
            b.ConfigureByConvention();
        });

        builder.Entity<DFApp.ElectricVehicle.ElectricVehicleChargingRecord>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "ElectricVehicleChargingRecord", DFAppConsts.DbSchema);
            b.Property(e => e.IsBelongToSelf)
            .HasDefaultValue(true);

            b.HasOne(e => e.Vehicle)
            .WithMany(e => e.ChargingRecords)
            .HasForeignKey(e => e.VehicleId);

            b.HasIndex(e => e.ChargingDate);
            b.HasIndex(e => e.VehicleId);
            b.ConfigureByConvention();
        });

        builder.Entity<DFApp.ElectricVehicle.GasolinePrice>(b =>
        {
            b.ToTable(DFAppConsts.DbTablePrefix + "GasolinePrice", DFAppConsts.DbSchema);

            b.HasIndex(e => e.Province);
            b.HasIndex(e => e.Date);
            b.HasIndex(e => new { e.Province, e.Date });
            b.ConfigureByConvention();
        });

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        //optionsBuilder.LogTo(System.Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
        //    .EnableSensitiveDataLogging();
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


}

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DFApp.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class DFAppDbContextFactory : IDesignTimeDbContextFactory<DFAppDbContext>
{
    public DFAppDbContext CreateDbContext(string[] args)
    {
        DFAppEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

#if DEBUG

#if SQLSERVER

        var builder = new DbContextOptionsBuilder<DFAppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
#else

        var builder = new DbContextOptionsBuilder<DFAppDbContext>()
            .UseSqlite(configuration.GetConnectionString("Default"));

#endif


#else

        var builder = new DbContextOptionsBuilder<DFAppDbContext>()
            .UseSqlite(configuration.GetConnectionString("Default"));

#endif



        return new DFAppDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../DFApp.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}

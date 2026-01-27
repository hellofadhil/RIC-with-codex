//using Core.Data;
using System.Text.Json;
using Core.Models.Entities;
using Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Models;

public class OneProDbContext(DbContextOptions<OneProDbContext> options) : DbContext(options)
{
    //public DbSet<Tenants>? Tenants { get; set; }
    //public DbSet<Employees>? Employees { get; set; }

    // Sesuai Kebutuhan
    public DbSet<Ric>? Rics { get; set; }

    #region Models
    public DbSet<SubMenu>? SubMenus { get; set; }
    public DbSet<BusinessProcess>? BusinessProcesses { get; set; }
    public DbSet<Application>? Applications { get; set; }
    public DbSet<Setting>? Settings { get; set; }
    #endregion

    #region ViewModels
    public DbSet<BusinessProcessResponse>? BusinessProcessResponses { get; set; }
    public DbSet<ApplicationResponse>? ApplicationResponses { get; set; }
    public DbSet<CompanyCodeResponse>? CompanyCodeResponses { get; set; }
    #endregion

    // Sesuai kebutuhan
    public DbSet<RicResponse>? RicResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ric>(e =>
        {
            e.Property(x => x.Hastag).HasStringListConversion();
            e.Property(x => x.AsIsProcessRasciFile).HasStringListConversion();
            e.Property(x => x.AlternatifSolusi).HasStringListConversion();
            e.Property(x => x.ToBeProcessBusinessRasciKkiFile).HasStringListConversion();
            e.Property(x => x.ExcpectedCompletionTargetFile).HasStringListConversion();
        });
    }
}

public static class ModelBuilderExtensions
{
    public static PropertyBuilder<List<string>?> HasStringListConversion(
        this PropertyBuilder<List<string>?> property
    )
    {
        return property.HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
        );
    }
}

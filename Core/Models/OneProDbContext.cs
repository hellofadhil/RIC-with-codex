using System.Text.Json;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Models;

public class OneProDbContext(DbContextOptions<OneProDbContext> options) : DbContext(options)
{
    // region Models
    public DbSet<Group>? Groups { get; set; }
    public DbSet<User>? Users { get; set; }
    public DbSet<FormRic>? FormRics { get; set; }
    public DbSet<FormRicApproval>? FormRicApprovals { get; set; }
    public DbSet<ReviewFormRic>? ReviewFormRics { get; set; }
    public DbSet<FormRicHistory>? FormRicHistories { get; set; }
    public DbSet<UndanganFormRic>? UndanganFormRics { get; set; }

    // region Ric Roll Out
    public DbSet<FormRicRollOut>? FormRicRollOuts { get; set; }
    public DbSet<FormRicRollOutApproval>? FormRicRollOutApprovals { get; set; }
    public DbSet<FormRicRollOutHistory>? FormRicRollOutHistories { get; set; }
    public DbSet<ReviewFormRicRollOut>? ReviewFormRicRollOuts { get; set; }

    // region ViewModels
    public DbSet<GroupResponse>? GroupResponses { get; set; }
    public DbSet<UserResponse>? UserResponses { get; set; }
    public DbSet<FormRicResponse>? FormRicResponses { get; set; }
    public DbSet<FormRicApprovalResponse>? FormRicApprovalResponses { get; set; }
    public DbSet<ReviewFormRicResponse>? ReviewFormRicResponses { get; set; }
    public DbSet<FormRicHistoryResponse>? FormRicHistoryResponses { get; set; }
    public DbSet<UndanganFormRicResponse>? UndanganFormRicResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FormRic>(e =>
        {
            e.Property(x => x.Hastag).HasStringListConversion();
            e.Property(x => x.AsIsProcessRasciFile).HasStringListConversion();
            e.Property(x => x.AlternatifSolusi).HasStringListConversion();
            e.Property(x => x.ToBeProcessBusinessRasciKkiFile).HasStringListConversion();
            e.Property(x => x.ExcpectedCompletionTargetFile).HasStringListConversion();
        });

        // builder.Entity<FormRicHistory>(e =>
        // {
        //     e.Property(x => x.Snapshot).HasColumnType("nvarchar(max)");
        //     e.Property(x => x.EditedFields).HasColumnType("nvarchar(max)");
        // });

        builder.Entity<FormRicHistory>(e =>
        {
            e.Property(x => x.SnapshotJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.EditedFieldsJson).HasColumnType("nvarchar(max)");
        });

        builder.Entity<FormRic>().Property(p => p.Status).HasConversion<string>();

        builder.Entity<FormRicApproval>().Property(p => p.Role).HasConversion<string>();

        builder.Entity<FormRicApproval>().Property(p => p.ApprovalStatus).HasConversion<string>();

        builder.Entity<ReviewFormRic>().Property(p => p.RoleReview).HasConversion<string>();

        #region FORM RIC ROLL OUT
        builder.Entity<FormRicRollOut>(e =>
        {
            // List<string> conversion
            e.Property(x => x.Hashtag).HasStringListConversion();
            e.Property(x => x.CompareWithAsIsHoldingProcessFiles).HasStringListConversion();
            e.Property(x => x.StkAsIsToBeFiles).HasStringListConversion();

            // Enum as string (biar readable)
            e.Property(x => x.Status).HasConversion<string>();

            // RELATIONSHIP MAPPING (INI YANG NGEFIX ERROR UserId/GroupId)
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.IdUser)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Group)
                .WithMany()
                .HasForeignKey(x => x.IdGroupUser)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FormRicRollOutApproval>(e =>
        {
            e.Property(x => x.Role).HasConversion<string>();
            e.Property(x => x.ApprovalStatus).HasConversion<string>();
        });

        builder.Entity<FormRicRollOutHistory>(e =>
        {
            e.Property(x => x.SnapshotJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.EditedFieldsJson).HasColumnType("nvarchar(max)");
        });

        builder.Entity<ReviewFormRicRollOut>(e =>
        {
            e.Property(x => x.RoleReview).HasConversion<string>();
        });

        #endregion
        // ========================================================
        // GROUP SEED
        // ========================================================
        var groupUser = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var groupBR = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var groupSARM = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var groupECS = Guid.Parse("40000000-0000-0000-0000-000000000004");

        builder
            .Entity<Group>()
            .HasData(
                new Group
                {
                    Id = groupUser,
                    NamaDivisi = "Enterprise Data Management",
                    NamaPerusahaan = "PT Pertamina (Persero)",
                },
                new Group
                {
                    Id = groupBR,
                    NamaDivisi = "Business RIC",
                    NamaPerusahaan = "PT Pertamina (Persero)",
                },
                new Group
                {
                    Id = groupSARM,
                    NamaDivisi = "Strategic & Risk Management",
                    NamaPerusahaan = "PT Pertamina (Persero)",
                },
                new Group
                {
                    Id = groupECS,
                    NamaDivisi = "Enterprise Corporate Strategy",
                    NamaPerusahaan = "PT Pertamina (Persero)",
                }
            );

        // ========================================================
        // GLOBAL PASSWORD HASH
        // ========================================================
        const string pass = "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu";

        // ========================================================
        // USER SEED (SEMUA ROLE)
        // ========================================================
        builder
            .Entity<User>()
            .HasData(
                // USER DIVISION
                new User
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000001"),
                    Name = "User Member",
                    Email = "user.member@pertamina.com",
                    Position = "Staff",
                    IdGroup = groupUser,
                    Role = Role.User_Member,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000002"),
                    Name = "Fadhil Rabbani",
                    Email = "user.pic@pertamina.com",
                    Position = "PIC",
                    IdGroup = groupUser,
                    Role = Role.User_Pic,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000003"),
                    Name = "User Manager",
                    Email = "user.manager@pertamina.com",
                    Position = "Manager",
                    IdGroup = groupUser,
                    Role = Role.User_Manager,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000004"),
                    Name = "User VP",
                    Email = "user.vp@pertamina.com",
                    Position = "Vice President",
                    IdGroup = groupUser,
                    Role = Role.User_VP,
                    PasswordHash = pass,
                },
                // BR DIVISION
                new User
                {
                    Id = Guid.Parse("22000000-0000-0000-0000-000000000004"),
                    Name = "BR Member",
                    Email = "br.member@pertamina.com",
                    Position = "Staff",
                    IdGroup = groupBR,
                    Role = Role.BR_Member,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("22000000-0000-0000-0000-000000000001"),
                    Name = "BR PIC",
                    Email = "br.pic@pertamina.com",
                    Position = "PIC",
                    IdGroup = groupBR,
                    Role = Role.BR_Pic,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("22000000-0000-0000-0000-000000000002"),
                    Name = "BR Manager",
                    Email = "br.manager@pertamina.com",
                    Position = "Manager",
                    IdGroup = groupBR,
                    Role = Role.BR_Manager,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("22000000-0000-0000-0000-000000000003"),
                    Name = "BR VP",
                    Email = "br.vp@pertamina.com",
                    Position = "Vice President",
                    IdGroup = groupBR,
                    Role = Role.BR_VP,
                    PasswordHash = pass,
                },
                // SARM DIVISION
                new User
                {
                    Id = Guid.Parse("33000000-0000-0000-0000-000000000004"),
                    Name = "SARM Member",
                    Email = "sarm.member@pertamina.com",
                    Position = "Staff",
                    IdGroup = groupSARM,
                    Role = Role.SARM_Member,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("33000000-0000-0000-0000-000000000001"),
                    Name = "SARM PIC",
                    Email = "sarm.pic@pertamina.com",
                    Position = "PIC",
                    IdGroup = groupSARM,
                    Role = Role.SARM_Pic,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("33000000-0000-0000-0000-000000000002"),
                    Name = "SARM Manager",
                    Email = "sarm.manager@pertamina.com",
                    Position = "Manager",
                    IdGroup = groupSARM,
                    Role = Role.SARM_Manager,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("33000000-0000-0000-0000-000000000003"),
                    Name = "SARM VP",
                    Email = "sarm.vp@pertamina.com",
                    Position = "Vice President",
                    IdGroup = groupSARM,
                    Role = Role.SARM_VP,
                    PasswordHash = pass,
                },
                // ECS DIVISION
                new User
                {
                    Id = Guid.Parse("44000000-0000-0000-0000-000000000004"),
                    Name = "ECS Member",
                    Email = "ecs.member@pertamina.com",
                    Position = "Staff",
                    IdGroup = groupECS,
                    Role = Role.ECS_Member,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("44000000-0000-0000-0000-000000000001"),
                    Name = "ECS PIC",
                    Email = "ecs.pic@pertamina.com",
                    Position = "PIC",
                    IdGroup = groupECS,
                    Role = Role.ECS_Pic,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("44000000-0000-0000-0000-000000000002"),
                    Name = "ECS Manager",
                    Email = "ecs.manager@pertamina.com",
                    Position = "Manager",
                    IdGroup = groupECS,
                    Role = Role.ECS_Manager,
                    PasswordHash = pass,
                },
                new User
                {
                    Id = Guid.Parse("44000000-0000-0000-0000-000000000003"),
                    Name = "ECS VP",
                    Email = "ecs.vp@pertamina.com",
                    Position = "Vice President",
                    IdGroup = groupECS,
                    Role = Role.ECS_VP,
                    PasswordHash = pass,
                }
            );
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

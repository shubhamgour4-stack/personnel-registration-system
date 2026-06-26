using Microsoft.EntityFrameworkCore;
using PRS.Core.Entities;

namespace PRS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PersonnelGuid> PersonnelGuids { get; set; }
        public DbSet<PersonnelGlobal> PersonnelGlobals { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<LineOfService> LinesOfService { get; set; }
        public DbSet<EmploymentStatus> EmploymentStatuses { get; set; }
        public DbSet<WorkOfficeLocation> WorkOffices { get; set; }
        public DbSet<GlobalCountry> GlobalCountries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<MftFileHistory> MftFileHistories { get; set; }
        public DbSet<MftFileStaging> MftFileStagings { get; set; }
        public DbSet<MftFileError> MftFileErrors { get; set; }
        public DbSet<PersonnelMftAudit> PersonnelMftAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // EXPLICIT MASTER DATA MAPPINGS
            // =========================================================
            modelBuilder.Entity<EmploymentStatus>().ToTable("Employment_Status").HasKey(e => e.Employment_Status_ID);
            modelBuilder.Entity<Grade>().ToTable("Grade").HasKey(g => g.Grade_ID);
            modelBuilder.Entity<LineOfService>().ToTable("Line_Of_Service").HasKey(l => l.LOS_ID);
            modelBuilder.Entity<WorkOfficeLocation>().ToTable("Work_Office_Location").HasKey(w => w.Work_Office_ID);
            modelBuilder.Entity<GlobalCountry>().ToTable("Global_Country").HasKey(c => c.Country_Code);

            // =========================================================
            // PERSONNEL MAPPINGS
            // =========================================================
            modelBuilder.Entity<PersonnelGuid>().ToTable("Personnel_Guid").HasKey(p => p.Unique_ID);
            modelBuilder.Entity<PersonnelGlobal>().ToTable("Personnel_Global").HasKey(pg => pg.ID);
            // Add this line inside your OnModelCreating method:
            modelBuilder.Entity<MftFileError>().ToTable("MFT_File_Errors").HasKey(e => e.ErrorId);

            // Explicit Primary Key Mappings for the complete MFT Processing Stack
            modelBuilder.Entity<MftFileHistory>().ToTable("MFT_File_History").HasKey(h => h.FileId);
            modelBuilder.Entity<MftFileStaging>().ToTable("MFT_File_Staging").HasKey(s => s.StagingId);
            modelBuilder.Entity<MftFileError>().ToTable("MFT_File_Errors").HasKey(e => e.ErrorId);
            modelBuilder.Entity<PersonnelMftAudit>().ToTable("Personnel_MFT_Audit").HasKey(a => a.AuditId);

            modelBuilder.Entity<PersonnelGlobal>()
                .HasOne(pg => pg.PersonnelGuid)
                .WithOne(p => p.PersonnelGlobal)
                .HasForeignKey<PersonnelGlobal>(pg => pg.Personnel_Guid_ID);

            // =========================================================
            // SECURITY MAPPINGS
            // =========================================================
            modelBuilder.Entity<User>().ToTable("Users").HasKey(u => u.User_ID);
            
            // Explicitly map the PasswordHash property to the SQL Password_Hash column!
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("Password_Hash");
            
            modelBuilder.Entity<Role>().ToTable("Roles").HasKey(r => r.Role_ID);
            modelBuilder.Entity<UserRole>().ToTable("User_Roles");

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.User_ID, ur.Role_ID });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.User_ID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.Role_ID);
        }
    }
}
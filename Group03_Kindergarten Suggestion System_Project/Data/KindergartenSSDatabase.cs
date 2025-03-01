using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Data
{
    public class KindergartenSSDatabase : IdentityDbContext<User, Role, string>
    {
        public KindergartenSSDatabase(DbContextOptions<KindergartenSSDatabase> options) : base(options) { }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<ChildAge> ChildAges { get; set; }
        public DbSet<CounsellingRequest> CounsellingRequests { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<EducationMethod> EducationMethods { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<ImageUrl> ImageUrls { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolEnrollment> SchoolEnrollments { get; set; }
        public DbSet<SchoolFacility> SchoolFacilities { get; set; }
        public DbSet<SchoolType> SchoolTypes { get; set; }
        public DbSet<SchoolUtility> SchoolUtilities { get; set; }
        public DbSet<Utility> Utilities { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<SchoolRating> SchoolRatings { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SchoolUtility>()
                .HasKey(su => new { su.SchoolId, su.UtilityId });

            modelBuilder.Entity<SchoolFacility>()
                .HasKey(sf => new { sf.SchoolId, sf.FacilityId });

            modelBuilder.Entity<School>()
                .HasOne(s => s.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<School>()
                .HasOne(s => s.Acceptor)
                .WithMany()
                .HasForeignKey(s => s.AcceptorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<School>()
                .HasOne(s => s.Address)
                .WithMany()
                .HasForeignKey(s => s.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.Province)
                .WithMany()
                .HasForeignKey(a => a.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.District)
                .WithMany()
                .HasForeignKey(a => a.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.Ward)
                .WithMany()
                .HasForeignKey(a => a.WardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<District>()
                .HasOne(d => d.Province)
                .WithMany()
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ward>()
                .HasOne(w => w.District)
                .WithMany()
                .HasForeignKey(w => w.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SchoolFacility>()
                .HasOne(sf => sf.School)
                .WithMany(s => s.SchoolFacilities)
                .HasForeignKey(sf => sf.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SchoolFacility>()
                .HasOne(sf => sf.Facility)
                .WithMany(f => f.SchoolFacilities)
                .HasForeignKey(sf => sf.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SchoolUtility>()
                .HasOne(su => su.School)
                .WithMany(s => s.SchoolUtilities)
                .HasForeignKey(su => su.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SchoolUtility>()
                .HasOne(su => su.Utility)
                .WithMany(u => u.SchoolUtilities)
                .HasForeignKey(su => su.UtilityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

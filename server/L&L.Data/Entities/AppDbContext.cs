using Microsoft.EntityFrameworkCore;

namespace L_L.Data.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        #region Dbset
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogRating> BlogRatings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderTracking> OrderTrackings { get; set; }
        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<DeliveryInfo> DeliveryInfos { get; set; }
        public DbSet<ServiceCost> ServiceCosts { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<PackageType> PackageTypes { get; set; }
        public DbSet<ShippingRate> ShippingRates { get; set; }
        public DbSet<VehiclePackageRelation> VehiclePackageRelations { get; set; }
        public DbSet<IdentityCard> IdentityCards { get; set; }
        public DbSet<LicenseDriver> LicenseDrivers { get; set; }

        public DbSet<Guess> Guess { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehiclePackageRelation>(e =>
            {
                e.ToTable("VehiclePackageRelation");
                e.HasKey(e => new { e.VehicleTypeId, e.PackageTypeId });

                e.HasOne(e => e.VehicleType)
                .WithMany(e => e.VehiclePackageRelations)
                .HasForeignKey(e => e.VehicleTypeId)
                .HasConstraintName("FK_VehiclePackageRelation1");

                e.HasOne(e => e.PackageType)
                .WithMany(e => e.VehiclePackageRelations)
                .HasForeignKey(e => e.PackageTypeId)
                .HasConstraintName("FK_VehiclePackageRelation2");

            });
        }
    }
}

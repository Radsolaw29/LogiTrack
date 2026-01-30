using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Entities
{
    public class LogiTrackDbContext: DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<TransportOrder> Orders { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public LogiTrackDbContext(DbContextOptions<LogiTrackDbContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Company>()
                .Property(r => r.TaxNumber)
                .IsRequired();

            modelBuilder.Entity<TransportOrder>()
                .Property(r => r.OrderName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Driver>()
                .Property(r => r.FirstName)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Driver>()
                .Property(r => r.LastName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Driver>()
                .Property(r => r.PersonalNumber)
                .IsRequired()
                .HasMaxLength(11);

            modelBuilder.Entity<Truck>()
                .Property(r => r.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(11);

            modelBuilder.Entity<Address>()
                .Property(r => r.PostalCode)
                .IsRequired()
                .HasMaxLength(12);

            modelBuilder.Entity<Address>()
                .Property(r => r.City)
                .HasMaxLength(50);

            modelBuilder.Entity<Address>()
                .Property(r => r.Street)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(r => r.Email)
                .IsRequired();

            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .IsRequired();

            modelBuilder.Entity<Company>()
                .HasOne(c => c.Address)
                .WithOne(a => a.Company)
                .HasForeignKey<Company>(c => c.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Drivers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Truck>()
                .HasOne(t => t.Company)
                .WithMany(c => c.Trucks)
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransportOrder>()
                .HasOne(t => t.Company)
                .WithMany(c => c.TransportOrders)
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransportOrder>()
                .HasOne(t => t.Driver)
                .WithMany(d => d.TransportOrders)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransportOrder>()
                .HasOne(t => t.Truck)
                .WithMany(d => d.TransportOrders)
                .HasForeignKey(t => t.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransportOrder>()
                .HasOne(t => t.PickupAddress)
                .WithMany(a => a.PickupOrders)
                .HasForeignKey(t => t.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransportOrder>()
                .HasOne(t => t.DeliveryAddress)
                .WithMany(a => a.DeliveryOrders)
                .HasForeignKey(t => t.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Truck>()
                .Property(t => t.CapacityTons)
                .HasPrecision(10, 2);
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(_connectionString);
        //}
    }
}

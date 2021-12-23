using BitFleet.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BitFleet.Data
{
    public class DataContext : IdentityDbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }

        public DbSet<CarCosts> CarCosts { get; set; }

        public DbSet<Ride> Rides { get; set; }

        public DbSet<Malfunction> Malfunctions { get; set; }

        public DbSet<VehicleService> VehicleServices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Car>()
                .HasMany<Ride>(x => x.Rides)
                .WithOne(x => x.Car)
                .Metadata.DeleteBehavior = DeleteBehavior.SetNull;
        }
    }
}
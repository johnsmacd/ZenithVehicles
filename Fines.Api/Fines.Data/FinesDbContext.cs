using Fines.Data.Models;
using Fines.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace Fines.Data;

public class FinesDbContext : DbContext
{
    public FinesDbContext(DbContextOptions<FinesDbContext> options) : base(options)
    {
    }

    public DbSet<FinesEntity> Fines { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<VehicleEntity> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<FinesEntity>()
            .HasOne(f => f.Vehicle)
            .WithMany()
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinesEntity>()
            .HasOne(f => f.Customer)
            .WithMany()
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        //// Seed data - moved to program.cs to allow for alternatibe unti test approach using in momeory database
        //modelBuilder.Entity<VehicleEntity>().HasData(VehicleSeedData.GetSeedData());
        //modelBuilder.Entity<CustomerEntity>().HasData(CustomerSeedData.GetSeedData());
        //modelBuilder.Entity<FinesEntity>().HasData(FinesSeedData.GetSeedData());
    }
}
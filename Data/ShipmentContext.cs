using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShipmentApi.Models;

namespace ShipmentApi.Data
{
    public class ShipmentContext(DbContextOptions<ShipmentContext> options) : DbContext(options) 
    {
        public DbSet<Shipment> Shipments { get; set; }

        // Configure the model to store the ShipmentStatus enum as a string in the database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shipment>()
                .Property(s => s.Status)
                .HasConversion<string>()   // store "Delivered", not 2
                .HasMaxLength(20);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using PetStore.Models;

namespace PetStore.DAL
{
    // TODO: move to repository pattern (2016)
    // TODO: use DI (2019)
    public class PetStoreContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // connection string is also in appsettings.json but that one didn't work on the build server
            optionsBuilder.UseSqlite("Data Source=petstore.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pet>().HasOne(p => p.Category).WithMany(c => c.Pets).HasForeignKey(p => p.CategoryId);
            modelBuilder.Entity<Order>().HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using UInsure.WebApi.DavidJamesForbes.DataModels;

namespace UInsure.WebApi.DavidJamesForbes.Data
{
    public class UinsureDbContext : DbContext
    {
        public UinsureDbContext(DbContextOptions<UinsureDbContext> options)
        : base(options)
        {

        }

        public DbSet<Policy> Policies { get; set; }
        public DbSet<Policyholder> Policyholders { get; set; }
        public DbSet<Property> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Policyholders)
                .WithOne(ph => ph.Policy)
                .HasForeignKey(ph => ph.PolicyId);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.Property)
                .WithOne(ph => ph.Policy)
                .HasForeignKey<Property>(p => p.PolicyId);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.Payment)
                .WithOne(p => p.Policy)
                .HasForeignKey<Payment>(p => p.PolicyId);
        }
    }
}

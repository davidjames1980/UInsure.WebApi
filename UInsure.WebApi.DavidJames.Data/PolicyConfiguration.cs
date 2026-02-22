using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UInsure.WebApi.DavidJames.Entities;

namespace UInsure.WebApi.DavidJames.Data
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.HasIndex(p => p.UniqueReference)
                   .IsUnique();

            builder.Property(p => p.UniqueReference)
                   .HasMaxLength(100)  
                   .IsRequired();
        }
    }
}

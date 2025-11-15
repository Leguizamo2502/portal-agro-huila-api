using Entity.Domain.Models.Implements.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entity.Infrastructure.DataInit.Order.ConsumerRatings
{
    public class ConsumerRatingSeeder : IEntityTypeConfiguration<ConsumerRating>
    {
        public void Configure(EntityTypeBuilder<ConsumerRating> builder)
        {
            builder.Property(r => r.Comment)
                .HasMaxLength(500);

            builder.HasIndex(r => r.OrderId)
                .IsUnique();

            builder.HasOne(r => r.Order)
                .WithOne(o => o.ConsumerRating)
                .HasForeignKey<ConsumerRating>(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Producer)
                .WithMany(p => p.ConsumerRatings)
                .HasForeignKey(r => r.ProducerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.User)
                .WithMany(u => u.ConsumerRatingsReceived)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

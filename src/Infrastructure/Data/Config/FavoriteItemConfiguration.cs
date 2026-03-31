using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.Infrastructure.Data.Config;

public class FavoriteItemConfiguration : IEntityTypeConfiguration<FavoriteItem>
{
    public void Configure(EntityTypeBuilder<FavoriteItem> builder)
    {
        builder.ToTable("FavoriteItems");

        builder.Property(f => f.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(f => f.CatalogItemId)
            .IsRequired();

        builder.Property(f => f.DateCreated)
            .IsRequired();

        builder.HasIndex(f => new { f.UserId, f.CatalogItemId })
            .IsUnique()
            .HasDatabaseName("IX_FavoriteItems_UserId_CatalogItemId");

        builder.HasIndex(f => f.CatalogItemId)
            .HasDatabaseName("IX_FavoriteItems_CatalogItemId");
    }
}

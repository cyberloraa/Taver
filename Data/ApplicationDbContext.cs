using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Taver.Models;

namespace Taver.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Artwork> Artworks => Set<Artwork>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Artwork>()
            .HasOne(a => a.Artist)
            .WithMany(a => a.Artworks)
            .HasForeignKey(a => a.ArtistID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

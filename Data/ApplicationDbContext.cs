using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Beboer> Beboere => Set<Beboer>();
        public DbSet<Leilighet> Leiligheter => Set<Leilighet>();
        public DbSet<Dugnad> Dugnader => Set<Dugnad>();
        public DbSet<Deltakelse> Deltakelser => Set<Deltakelse>();
        public DbSet<Sameie> Sameier { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Beboer -> Identity
            builder.Entity<Beboer>()
                .HasOne(b => b.ApplicationUser)
                .WithMany()
                .HasForeignKey(b => b.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Beboer -> Leilighet
            builder.Entity<Beboer>()
                .HasOne(b => b.Leilighet)
                .WithMany(l => l.Beboere)
                .HasForeignKey(b => b.LeilighetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unik seksjonsnummer
            builder.Entity<Leilighet>()
                .HasIndex(l => l.Seksjonsnummer)
                .IsUnique();

            // Unik e-post
            builder.Entity<Beboer>()
                .HasIndex(b => b.Epost)
                .IsUnique();

            builder.Entity<Deltakelse>()
                .HasOne(d => d.Beboer)
                .WithMany(b => b.Deltakelser)
                .HasForeignKey(d => d.BeboerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Deltakelse>()
                .HasOne(d => d.Dugnad)
                .WithMany(d => d.Deltakelser)
                .HasForeignKey(d => d.DugnadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Deltakelse>()
                .HasIndex(d => new { d.DugnadId, d.BeboerId })
                .IsUnique();
        }
    }
}
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
         public DbSet<Sameie> Sameier { get; set; }
        public DbSet<Dugnadstime> Dugnadstimer { get; set; }
        public DbSet<Innstillinger> Innstillinger => Set<Innstillinger>();
        public DbSet<Oppgave> Oppgaver { get; set; }
        public DbSet<OppgavePamelding> OppgavePameldinger { get; set; }
        public DbSet<Timeforing> Timeforinger { get; set; }
        public DbSet<Endringslogg> Endringslogger { get; set; }
        public DbSet<BoardMessage> BoardMessages { get; set; }
        public DbSet<Arsstatistikk> Arsstatistikker => Set<Arsstatistikk>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Endringslogg>()
            .HasOne(e => e.Bruker)
            .WithMany()
            .HasForeignKey(e => e.BrukerId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OppgavePamelding>()
            .ToTable("OppgavePamelding");

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
            
            // Unik e-post
            builder.Entity<Beboer>()
                .HasIndex(b => b.Epost)
                .IsUnique();

            // Dugnadstime -> Dugnad
            builder.Entity<Dugnadstime>()
                .HasOne(dt => dt.Dugnad)
                .WithMany(d => d.Dugnadstimer)
                .HasForeignKey(dt => dt.DugnadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dugnadstime -> Beboer
            builder.Entity<Dugnadstime>()
                .HasOne(dt => dt.Beboer)
                .WithMany(b => b.Dugnadstimer)
                .HasForeignKey(dt => dt.BeboerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Oppgavepåmelding -> Oppgave
            builder.Entity<OppgavePamelding>()
                .HasOne(p => p.Oppgave)
                .WithMany(o => o.Pameldinger)
                .HasForeignKey(p => p.OppgaveId);

            // Oppgavepåmelding -> Beboer
            builder.Entity<OppgavePamelding>()
                .HasOne(p => p.Beboer)
                .WithMany(b => b.OppgavePameldinger)
                .HasForeignKey(p => p.BeboerId);

            builder.Entity<Endringslogg>()
           .HasOne(e => e.Beboer)
           .WithMany()
           .HasForeignKey(e => e.BeboerId)
           .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
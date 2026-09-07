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
        public DbSet<ArsstatistikkBygg> ArsstatistikkBygg => Set<ArsstatistikkBygg>();
        public DbSet<OppgaveBilde> OppgaveBilder { get; set; }
        public DbSet<DugnadBilde> DugnadBilder { get; set; }
        public DbSet<EpostVarsel> EpostVarsler { get; set; }
          public DbSet<EpostVarselMottaker> EpostVarselMottakere { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Endringslogg>()
            .HasOne(e => e.Bruker)
            .WithMany()
            .HasForeignKey(e => e.BrukerId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ArsstatistikkBygg>()
     .HasIndex(x => new
     {
         x.Aar,
         x.ByggKode
     })
     .IsUnique();

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

            // Bilder -> Oppgaver
            builder.Entity<OppgaveBilde>()
           .HasOne(b => b.Oppgave)
           .WithMany(o => o.Bilder)
           .HasForeignKey(b => b.OppgaveId)
           .OnDelete(DeleteBehavior.Cascade);

            // Bilder -> Fellesdugnader
            builder.Entity<DugnadBilde>()
           .HasOne(b => b.Dugnad)
           .WithMany(d => d.Bilder)
           .HasForeignKey(b => b.DugnadId)
           .OnDelete(DeleteBehavior.Cascade);

            // Epost -> Varsel
            builder.Entity<EpostVarsel>()
           .HasOne(e => e.Oppgave)
           .WithMany(o => o.EpostVarsler)
           .HasForeignKey(e => e.OppgaveId)
           .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<EpostVarsel>()
                .HasOne(e => e.Dugnad)
                .WithMany()
                .HasForeignKey(e => e.DugnadId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<EpostVarsel>()
                .HasOne(e => e.SendtAvBruker)
                .WithMany()
                .HasForeignKey(e => e.SendtAvBrukerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EpostVarselMottaker>()
                .HasOne(m => m.EpostVarsel)
                .WithMany(e => e.Mottakere)
                .HasForeignKey(m => m.EpostVarselId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EpostVarselMottaker>()
                .HasOne(m => m.Beboer)
                .WithMany()
                .HasForeignKey(m => m.BeboerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
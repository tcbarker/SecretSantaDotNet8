using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data.Models;

namespace SecretSanta.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Email> Emails { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //optionsBuilder.ConfigureWarnings(w => w.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder){
        base.OnModelCreating(builder);

        builder.Entity<CampaignMember>()
                    .HasOne(mem => mem.DisplayEmail)
                    .WithMany(em => em.DisplayMembers)
                    .HasForeignKey(mem => mem.DisplayEmailId);

        builder.Entity<CampaignMember>()
                    .HasOne(mem => mem.ChosenEmail)
                    .WithMany(em => em.ChosenMembers)
                    .HasForeignKey(mem => mem.ChosenEmailId);

        builder.Entity<CampaignMember>()
                    .HasOne(mem => mem.VerifiedByEmail)
                    .WithMany(em => em.VerifiedByMembers)
                    .HasForeignKey(mem => mem.VerifiedByEmailId);

    }

}


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data.Models;

namespace SecretSanta.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Email> Emails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //optionsBuilder.ConfigureWarnings(w => w.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning));
    }

}


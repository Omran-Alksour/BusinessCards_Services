using Domain.Abstractions;
using Domain.Entities;
using Domain.Entities.BusinessCard;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public partial class ApplicationDbContext : IdentityDbContext<BusinessCard>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
            OnModelCreatingPartial(modelBuilder);

            _ = modelBuilder.Entity<BusinessCard>()
                            .HasIndex(c => c.Email)
                            .IsUnique();

            base.OnModelCreating(modelBuilder);

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public DbSet<BusinessCard> BusinessCards { get; set; }

    }

}
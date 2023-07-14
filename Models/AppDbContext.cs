
using Microsoft.EntityFrameworkCore;
using App.Models.Contacts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using App.Models.Blog;

namespace App.Models;
public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {


    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug);
        });
    }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Category> Categories { get; set; }
}
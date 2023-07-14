
using Microsoft.EntityFrameworkCore;
using App.Models.Contacts;
namespace App.Models;
public class AppDbContext : DbContext{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options){


    }
    protected  override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder){

    }
    public DbSet<Contact> Contacts{get;set;}

}
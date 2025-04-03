using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Person> Person {get;set;}
        public DbSet<Marriage> Marriage { get;set;}
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .HasOne(p => p.Father)
                .WithMany()
                .HasForeignKey(p => p.FatherId)
                .OnDelete(DeleteBehavior.Restrict);  // Указываем стратегию удаления

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Mother)
                .WithMany()
                .HasForeignKey(p => p.MotherId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Marriage>()
                .HasOne(p => p.Husband)
                .WithMany()
                .HasForeignKey(p => p.HusbandId)
                .OnDelete(DeleteBehavior.Restrict);  // Указываем стратегию удаления

            modelBuilder.Entity<Marriage>()
                .HasOne(p => p.Wife)
                .WithMany()
                .HasForeignKey(p => p.WifeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=FamilyGuy;Username=postgres;Password=12345678");
        }
    }
}

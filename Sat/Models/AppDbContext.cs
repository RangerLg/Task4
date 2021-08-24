using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task4Core.Models;

namespace Sat.Models
{
    public class AppDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .ToTable("Items");
            
        }
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
           
        }
        
        public DbSet<Item> Items { get; set; }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<CommentModel> Comments { get; set; }

        public DbSet<Tags> Tags { get; set; }
    }
}
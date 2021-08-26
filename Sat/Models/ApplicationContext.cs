using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Task4Core.Models;

namespace Task4Core.Models
{
    public class ApplicationContext:IdentityDbContext<User>
    {
        private IConfiguration Configuration;

        public ApplicationContext(DbContextOptions<ApplicationContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
            base.OnConfiguring(optionsBuilder);
        }

    }
}

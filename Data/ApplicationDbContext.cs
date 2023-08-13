using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ADVERTISEMENT.Models;
using System;

namespace ADVERTISEMENT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ADVERTISEMENT.Models.Category> Category { get; set; }
        public DbSet<ADVERTISEMENT.Models.Location> Location { get; set; }
        public DbSet<ADVERTISEMENT.Models.Advertisement> Advertisement { get; set; }     
        public DbSet<ADVERTISEMENT.Models.Customer> Customer { get; set; }
        public DbSet<ADVERTISEMENT.Models.Comment> Comment { get; set; }     
        public DbSet<ADVERTISEMENT.Models.ActivityLog> ActivityLog { get; set; }
        public DbSet<ADVERTISEMENT.Models.Contact> Contact { get; set; }
        public DbSet<ADVERTISEMENT.Models.Rely> Rely { get; set; }
     
    }
}

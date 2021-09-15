using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ColmanGym.Areas.Identity.Data;

namespace ColmanGym.Data
{
    public class ColmanGymContext : DbContext
    {
        public ColmanGymContext (DbContextOptions<ColmanGymContext> options)
            : base(options)
        {
        }

        public DbSet<ColmanGym.Models.Training> Trainings { get; set; }
        public DbSet<ColmanGym.Models.Meeting> Meetings { get; set; }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<IdentityRole> AspNetRoles { get; set; }
    }
}

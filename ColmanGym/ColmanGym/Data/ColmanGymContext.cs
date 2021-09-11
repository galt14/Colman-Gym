using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ColmanGym.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ColmanGym.Data
{
    public class ColmanGymContext : IdentityDbContext<User>
    {
        public ColmanGymContext (DbContextOptions<ColmanGymContext> options)
            : base(options)
        {
        }

        public DbSet<ColmanGym.Models.Training> Trainings { get; set; }

        public DbSet<ColmanGym.Models.Meeting> Meetings { get; set; }
    }
}

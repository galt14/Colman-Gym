using ColmanGym.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColmanGym.Data
{
    public class IdentityColmanGymContext : IdentityDbContext<User>
    {
        public IdentityColmanGymContext(DbContextOptions<IdentityColmanGymContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}

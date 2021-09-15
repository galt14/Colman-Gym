using ColmanGym.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

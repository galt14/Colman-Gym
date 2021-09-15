using ColmanGym.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ColmanGym.Data
{
    public class ColmanGymIdentityContext : IdentityDbContext<ApplicationUser>
    {
        public ColmanGymIdentityContext(DbContextOptions<ColmanGymIdentityContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}

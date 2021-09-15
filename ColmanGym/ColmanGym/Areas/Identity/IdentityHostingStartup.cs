using ColmanGym.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ColmanGym.Areas.Identity.Data;

[assembly: HostingStartup(typeof(ColmanGym.Areas.Identity.IdentityHostingStartup))]
namespace ColmanGym.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<ColmanGymIdentityContext>(options =>
                    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ColmanGymContext-7dfba827-38a0-4e15-9548-005bf0a50357;Trusted_Connection=True;MultipleActiveResultSets=true"));

                services.AddDefaultIdentity<ApplicationUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ColmanGymIdentityContext>()
                    .AddDefaultUI();
            });
        }
    }
}

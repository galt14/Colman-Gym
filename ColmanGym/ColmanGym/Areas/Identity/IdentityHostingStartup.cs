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
                    options.UseSqlite("Data Source=App_Data/data.db"));

                services.AddDefaultIdentity<ApplicationUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ColmanGymIdentityContext>()
                    .AddDefaultUI();
            });
        }
    }
}

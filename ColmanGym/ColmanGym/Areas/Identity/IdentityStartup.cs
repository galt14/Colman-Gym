using ColmanGym.Areas.Identity.Models;
using ColmanGym.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: HostingStartup(typeof(ColmanGym.Areas.Identity.IdentityStartup))]
namespace ColmanGym.Areas.Identity
{
    public class IdentityStartup : IHostingStartup
    {
        public IdentityStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<IdentityColmanGymContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ColmanGymContext")));

                services.AddDefaultIdentity<User>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<IdentityColmanGymContext>()
                    .AddDefaultUI();
            });
        }
    }
}

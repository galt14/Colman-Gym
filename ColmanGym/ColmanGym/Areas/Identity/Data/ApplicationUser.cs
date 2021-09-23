using Microsoft.AspNetCore.Identity;

namespace ColmanGym.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }

        [PersonalData]
        public string LastName { get; set; }

        [PersonalData]
        public bool IsTrainer { get; set; }

        [PersonalData]
        public string Address { get; set; }

        [PersonalData]
        public string City { get; set; }

        [PersonalData]
        public string Gender { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColmanGym.Models
{
    public class User : IdentityUser
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

using ColmanGym.Areas.Identity.Data;
using ColmanGym.Models;
using System;
using System.Linq;

namespace ColmanGym.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ColmanGymContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Trainings.Any())
            {
                var trainings = new Training[]
                {
                    new Training {Name="Body Pump", Target="Increase muscels"},
                    new Training {Name= "Kickbox", Target="Burning calories"},
                };

                foreach (Training t in trainings)
                {
                    context.Trainings.Add(t);
                }

                context.SaveChanges();
            }

            if (!context.AspNetUsers.Any(t => t.IsTrainer))
            {
                var users = new ApplicationUser[]
                {
                    new ApplicationUser {FirstName="Maor", LastName="Levy", PhoneNumber="0544444444", Email="maor@mail.com", Gender="male", Address="Street1", City="Tel Aviv", IsTrainer=true},
                    new ApplicationUser {FirstName="Itay", LastName="Cohen", PhoneNumber="0522222222", Email="itay@mail.com", Gender="male", Address="Street2", City="Tel Aviv", IsTrainer=true},
                };

                foreach (ApplicationUser user in users)
                {
                    context.AspNetUsers.Add(user);
                }

                context.SaveChanges();
            }

            if (!context.Meetings.Any())
            {
                var trainer = context.AspNetUsers.Where(user => user.IsTrainer).First();
                var training = context.Trainings.First();

                var meetings = new Meeting[]
                {
                    new Meeting {Trainer=trainer, Training=training, Date=DateTime.Now.AddHours(1)},
                    new Meeting {Trainer=trainer, Training=training, Date=DateTime.Now.AddHours(2)},
                };

                foreach (Meeting m in meetings)
                {
                    context.Meetings.Add(m);
                }

                context.SaveChanges();
            }
        }
    }
}

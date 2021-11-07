using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColmanGym.Data;
using ColmanGym.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ColmanGym.Areas.Identity.Data;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using TweetSharp;
using ColmanGym.MeetingsClusterer;

namespace ColmanGym.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ColmanGymContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient client;
        private readonly Clusterer _clusterer;
        static readonly string trainByCityPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "graph_data", "TrainbyCity.csv");
        static readonly string CountMeetingsPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "graph_data", "CountMeetingbyType.csv");

        public MeetingsController(ColmanGymContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _clusterer = new Clusterer(_context);
            _clusterer.CreateModel();

            TrainbyCityGraph();
            CountMeetingbyTypeGraph();

            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

           
        }

        // GET: Meetings
        public async Task<IActionResult> Index(int? trainingId)
        {
            List<Meeting> meetings;
            if (trainingId != null)
            {
                meetings = await _context.Meetings
                    .Include(m => m.Training)
                    .Include(m => m.Trainer)
                    .Where(m => m.TrainingID == trainingId).ToListAsync();
            }
            else
            {
                meetings = await _context.Meetings
                    .Include(m => m.Training)
                    .Include(m => m.Trainer).ToListAsync();
            }
            return View(meetings);
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested meeting is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var meeting = await _context.Meetings
                .Include(m => m.Training)
                .Include(m => m.Trainer)
                .FirstOrDefaultAsync(m => m.MeetID == id);

            if (meeting == null)
            {
                ViewData["NotFound"] = "The requested meeting is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            if (User.IsInRole("Trainer") && _userManager.GetUserId(User) != meeting.TrainerID)
            {
                ViewData["AccessDenied"] = true;
            }

            try
            {
                MeetingPrediction prediction = _clusterer.Predict(meeting);
                List<Meeting> MeetingsInSameCluster = await GetMeetingsInCluster(prediction.PredictedClusterId);
                MeetingsInSameCluster.Remove(meeting);
                ViewBag.otherMeetings = MeetingsInSameCluster;
            }
            catch
            {
                Console.Write("failed to cluster");
            }
            await GetMeetingCoordinates(meeting.Trainer.Address, meeting.Trainer.City);

            return View(meeting);
        }

        // GET: Meetings/Create
        [Authorize(Roles = "Admin, Trainer")]
        public IActionResult Create()
        {
            ViewData["TrainingId"] = new SelectList(_context.Trainings, "TrainingId", "Name");
            ViewData["TrainerId"] = GetRelevantTrainersToSelect();

            return View();
        }

        // POST: Meetings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Trainer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MeetID,TrainingID,TrainerID,Date,Price")] Meeting meeting)
        {
            if (ModelState.IsValid)
            {
                var trainer = await _context.AspNetUsers.FindAsync(meeting.TrainerID);
                if (trainer == null)
                {
                    ViewData["NotFound"] = "The requested trainer is no longer available";
                    return View("~/Views/Home/Index.cshtml");
                }
                var trainingType = await _context.Trainings.FindAsync(meeting.TrainingID);
                if (trainingType == null)
                {
                    ViewData["NotFound"] = "The requested trainning method is no longer available";
                    return View("~/Views/Home/Index.cshtml");
                }

                _context.Add(meeting);
                await _context.SaveChangesAsync();

                string key = "akxTSxiQEn71TEr5rVGBR0h5X";
                string secret = "m64Zmb17CnQkU9neanVycVJE2t9ZZ2GxvDmsOJUZMqMR8Vg3C4";
                string token = "1456947607692054532-N2SeTDwPmcBFWKGhGcYnKAxlJtzvfS";
                string tokenSecret = "EUAKnJolGcw75OjmXCKNyYIaeEIEJuByaKhUkaTT2XV6r";
                var service = new TweetSharp.TwitterService(key, secret);
                service.AuthenticateWith(token, tokenSecret);
                TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions());
                var traintype = await _context.Trainings
                   .FirstOrDefaultAsync(m => m.TrainingId == meeting.TrainingID);
                string message = string.Format("New {0} meeting is available at {1} {2}", traintype.Name, meeting.Date.ToShortDateString(), meeting.Date.ToShortTimeString());
                //var result = service.SendTweet(new SendTweetOptions
                //{
                //    Status = message
                //});


                return RedirectToAction(nameof(Index));
            }

            ViewData["TrainingID"] = new SelectList(_context.Trainings, "TrainingId", "Name");
            ViewData["TrainerID"] = GetRelevantTrainersToSelect();

            return View(meeting);
        }

        // GET: Meetings/Edit/5
        [Authorize(Roles = "Admin, Trainer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested meeting is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var meeting = await _context.Meetings
                            .Include(m => m.Training)
                            .Include(m => m.Trainer)
                            .FirstOrDefaultAsync(m => m.MeetID == id);

            if (meeting == null)
            {
                ViewData["NotFound"] = "The requested meeting is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            ViewData["TrainingID"] = new SelectList(_context.Trainings, "TrainingId", "Name");
            ViewData["TrainerID"] = GetRelevantTrainersToSelect();

            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Trainer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetID,TrainingID,TrainerID,Date,Price")] Meeting meeting)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingMeeting = await _context.Meetings.FirstOrDefaultAsync(m => m.MeetID == id);
                    if (User.IsInRole("Trainer") && existingMeeting != null && existingMeeting.TrainerID != this._userManager.GetUserId(User))
                    {
                        ViewData["AccessDenied"] = true;
                        return View(meeting);
                    }

                    var meetingToUpdate = await _context.Meetings.Include(m => m.Training).Include(m => m.Trainer).FirstOrDefaultAsync(m => m.MeetID == id);
                    meetingToUpdate.Price = meeting.Price;
                    meetingToUpdate.Date = meeting.Date;
                    meetingToUpdate.TrainingID = meeting.TrainingID;
                    meetingToUpdate.TrainerID = meeting.TrainerID;
                    _context.Update(meetingToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingExists(meeting.MeetID))
                    {
                        ViewData["NotFound"] = "The requested meeting is no longer available";
                        return View("~/Views/Home/Index.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["TrainingID"] = new SelectList(_context.Trainings, "TrainingId", "Name");
            ViewData["TrainerID"] = this.GetRelevantTrainersToSelect();

            return View(meeting);
        }

        // GET: Meetings/Delete/5
        [Authorize(Roles = "Admin, Trainer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested meeting is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var meeting = await _context.Meetings
                .Include(m => m.Training)
                .Include(m => m.Trainer)
                .FirstOrDefaultAsync(m => m.MeetID == id);

            if (meeting == null)
            {
                ViewData["NotFound"] = "The requested meeting is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            return View(meeting);
        }

        // POST: Meetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Trainer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool MeetingExists(int id)
        {
            return _context.Meetings.Any(e => e.MeetID == id);
        }

        private SelectList GetRelevantTrainersToSelect()
        {
            if (User.IsInRole("Trainer"))
            {
                var currentUser = _context.AspNetUsers.Find(this._userManager.GetUserId(User));

                return new SelectList(new List<ApplicationUser> { currentUser }, "Id", "Email");
            }

            return new SelectList(_context.AspNetUsers.Where(t => t.IsTrainer).ToList(), "Id", "Email");
        }

        private async Task GetMeetingCoordinates(string address, string city)
        {

            string completeAddress = address + "," + city;
            string apiKey = "16e1cbdfd87d0a";
            string requestUrl = "https://eu1.locationiq.com/v1/search.php?key=" + apiKey + "&q=" + completeAddress + "&format=json";

            HttpResponseMessage response = await client.GetAsync(requestUrl);
            HttpContent responseContent = response.Content;
            using var reader = new StreamReader(await responseContent.ReadAsStreamAsync());
            string json = await reader.ReadToEndAsync();
            try
            {
                List<Location> coordinates = JsonConvert.DeserializeObject<List<Location>>(json);


                ViewBag.lat = coordinates[0].Lat;
                ViewBag.lng = coordinates[0].Lon;
            }
            catch (Exception e)
            {
                completeAddress = "Eli Weisel 2, Rishon LeTsiyon";
                requestUrl = "https://eu1.locationiq.com/v1/search.php?key=" + apiKey + "&q=" + completeAddress + "&format=json";

                response = await client.GetAsync(requestUrl);
                responseContent = response.Content;
                using var reader2 = new StreamReader(await responseContent.ReadAsStreamAsync());
                json = await reader2.ReadToEndAsync();

                List<Location> coordinates = JsonConvert.DeserializeObject<List<Location>>(json);


                ViewBag.lat = coordinates[0].Lat;
                ViewBag.lng = coordinates[0].Lon;
            }
        }
        public void TrainbyCityGraph() //create data for the first graph
        { //calculate the train meetings  per city
            var trainPerCity = from s in _context.Meetings
                               join a in _context.AspNetUsers
                               on s.TrainerID equals a.Id
                               group a by a.City into city_count
                               select new
                               {
                                   key = city_count.Key,
                                   Count = city_count.Count()
                               };
            try
            {
                StreamWriter writer;
                FileStream file = System.IO.File.Open(trainByCityPath, FileMode.OpenOrCreate);
                file.Close();
                writer = new StreamWriter(trainByCityPath);
                writer.Write("City" + "," + "train\n");
                foreach (var s in trainPerCity)
                {
                    writer.Write(s.key + "," + s.Count + "\n");
                    writer.Flush();
                }
                file.Close();
                writer.Close();
            }
            catch
            {
            }

        }
        public void CountMeetingbyTypeGraph() //create data for the second graph
        {
            //calculate the amount of meetings by the number of training types
            var MeetingbyType = from s in _context.Meetings
                                join ty in _context.Trainings
                                on s.TrainingID equals ty.TrainingId
                                group ty by ty.Name into meetings_count
                                select new
                                {
                                    key = meetings_count.Key,
                                    Count = meetings_count.Count()
                                };

            try
            {
                StreamWriter writer;
                FileStream file = System.IO.File.Open(CountMeetingsPath, FileMode.OpenOrCreate);
                file.Close();
                writer = new StreamWriter(CountMeetingsPath);
                writer.Write("Training_type" + "," + "AmountOfMeetings\n");
                foreach (var a in MeetingbyType)
                {
                    writer.Write(a.key + "," + a.Count + "\n");
                    writer.Flush();
                }
                file.Close();
                writer.Close();
            }
            catch
            {
            }
        }
        async private Task<List<Meeting>> GetMeetingsInCluster(uint clusterId)
        {
            List<Meeting> meetings_in_same_cluster = new List<Meeting>();
            foreach (Meeting m in await _context.Meetings.Include(m => m.Trainer).Include(m => m.Training).ToListAsync())
            {
                try
                {
                    MeetingPrediction meetingPrediction = _clusterer.Predict(m);
                    if (meetingPrediction.PredictedClusterId == clusterId)

                        meetings_in_same_cluster.Add(m);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return meetings_in_same_cluster;
        }
    }
}

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

namespace ColmanGym.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ColmanGymContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient client;

        public MeetingsController(ColmanGymContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

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
    }
}

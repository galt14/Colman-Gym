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

namespace ColmanGym.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ColmanGymContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public MeetingsController(ColmanGymContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<IActionResult> Create([Bind("MeetId,TrainingId,TrainerId,Date,Price")] Meeting meeting)
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
        public async Task<IActionResult> Edit(int id, [Bind("MeetId,TrainingId,TrainerId,Date,Price")] Meeting meeting)
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
    }
}

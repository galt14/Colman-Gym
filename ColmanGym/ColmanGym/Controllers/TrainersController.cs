using ColmanGym.Areas.Identity.Data;
using ColmanGym.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColmanGym.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ColmanGymContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainersController(ColmanGymContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.AspNetUsers.Where(user => user.IsTrainer).ToListAsync();
            return View(trainers);
        }

        public async Task<IActionResult> Search(string searchString)
        {
            var users = from a in _context.AspNetUsers.Where(user => user.IsTrainer)
                        select a;
            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.City.Contains(searchString) || s.Gender.Contains(searchString) || s.FirstName.Contains(searchString) || s.LastName.Contains(searchString) || s.Email.Contains(searchString) || s.Address.Contains(searchString) || s.PhoneNumber.Contains(searchString));
            }
            var ids = users.Select(s => s.Id);
            var usersToShow = await _context.AspNetUsers.Where(x => ids.Contains(x.Id)).ToListAsync();

            return View("~/Views/Trainers/index.cshtml", usersToShow);
        }

        public async Task<IActionResult> MultipleSearch(string fname, string gender, string city, string phonenumber)
        {

            var trainer = from a in _context.AspNetUsers.Where(user => user.IsTrainer)
                          select a;


            if (!String.IsNullOrEmpty(fname) || !String.IsNullOrEmpty(city) || !String.IsNullOrEmpty(gender) || !String.IsNullOrEmpty(phonenumber))
            {
                if (!String.IsNullOrEmpty(fname))
                {
                    trainer = trainer.Where(s => s.FirstName.Contains(fname));
                }
                if (!String.IsNullOrEmpty(city))
                {
                    trainer = trainer.Where(s => s.City.Contains(city));
                }
                if (!String.IsNullOrEmpty(gender))
                {
                    trainer = trainer.Where(s => s.Gender.Contains(gender));
                }
                if (!String.IsNullOrEmpty(phonenumber))
                {
                    trainer = trainer.Where(s => s.PhoneNumber.Contains(phonenumber));
                }

            }

            var ids = trainer.Select(s => s.Id);
            var trainerSearch = await _context.AspNetUsers.Where(s => ids.Contains(s.Id)).ToListAsync();

            return View("~/Views/Trainers/Index.cshtml", trainerSearch);
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested trainer is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var trainer = await _context.AspNetUsers
                .FirstOrDefaultAsync(m => m.Id == id && m.IsTrainer);

            if (trainer == null)
            {
                ViewData["NotFound"] = "The requested trainer is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            return View(trainer);
        }

        // GET: Trainers/Create
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,FirstName,LastName,PhoneNumber, Gender,Address,City")] ApplicationUser trainer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.AspNetUsers.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return View(trainer);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }

        // GET: Trainers/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested trainer is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var trainer = await _context.AspNetUsers.FindAsync(id);
            if (trainer == null)
            {
                ViewData["NotFound"] = "The requested trainer is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }
            return View(trainer);
        }

        // POST: Trainers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,FirstName,LastName,PhoneNumber,Gender,Address,City,IsTrainer")] ApplicationUser trainer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        ViewData["NotFound"] = "The requested trainer is no longer available";
                        return View("~/Views/Home/Index.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested trainer is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var trainer = await _context.AspNetUsers
                .FirstOrDefaultAsync(m => m.Id == id && m.IsTrainer);

            if (trainer == null)
            {
                ViewData["NotFound"] = "The requested trainer is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var trainer = await _context.AspNetUsers.FindAsync(id);
            _context.Meetings.RemoveRange(_context.Meetings.Where(m => m.TrainerID == trainer.Id));
            _context.AspNetUsers.Remove(trainer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Meetings
        [Authorize(Roles = "Trainer")]
        public async Task<IActionResult> Meetings()
        {
            var trainerId = _userManager.GetUserId(User);
            var meetings = await _context.Meetings
                    .Include(m => m.TrainingID)
                    .Include(m => m.Trainer)
                    .Where(m => m.TrainerID == trainerId)
                    .ToListAsync();
            return View(meetings);
        }

        private bool TrainerExists(string id)
        {
            return _context.AspNetUsers.Any(e => e.Id == id && e.IsTrainer);
        }
    }
}

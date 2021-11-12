using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmanGym.Data;
using ColmanGym.Models;
using Microsoft.AspNetCore.Authorization;

namespace ColmanGym.Controllers
{
    public class TrainingsController : Controller
    {
        private readonly ColmanGymContext _context;

        public TrainingsController(ColmanGymContext context)
        {
            _context = context;
        }

        // GET: Trainings
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trainings.ToListAsync());
        }

        public async Task<IActionResult> Search(string searchString)
        {
            var trainings = from a in _context.Trainings
                        select a;

            if (!string.IsNullOrEmpty(searchString))
            {
                trainings = trainings.Where(s => s.Name.Contains(searchString) || s.Target.Contains(searchString));
            }

            var ids = trainings.Select(s => s.TrainingId);
            var trainingsToShow = await _context.Trainings.Where(x => ids.Contains(x.TrainingId)).ToListAsync();

            return View("~/Views/Trainings/index.cshtml", trainingsToShow);
        }

        // GET: Trainings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested training is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var training = await _context.Trainings
                .FirstOrDefaultAsync(m => m.TrainingId == id);
            if (training == null)
            {
                ViewData["NotFound"] = "The requested training is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            return View(training);
        }

        // GET: Trainings/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trainings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainingId,Name,Target")] Training training)
        {
            if (ModelState.IsValid)
            {
                _context.Add(training);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(training);
        }

        // GET: Trainings/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested training is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var training = await _context.Trainings.FindAsync(id);
            if (training == null)
            {
                ViewData["NotFound"] = "The requested training is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }
            return View(training);
        }

        // POST: Trainings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TrainingId,Name,Target")] Training training)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(training);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainingExists(training.TrainingId))
                    {
                        ViewData["NotFound"] = "The requested training is no longer available";
                        return View("~/Views/Home/Index.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(training);
        }

        // GET: Trainings/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ViewData["NotFound"] = "The requested training is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            var training = await _context.Trainings
                .FirstOrDefaultAsync(m => m.TrainingId == id);

            if (training == null)
            {
                ViewData["NotFound"] = "The requested training is no longer available";
                return View("~/Views/Home/Index.cshtml");
            }

            return View(training);
        }

        // POST: Trainings/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var training = await _context.Trainings.FindAsync(id);

            _context.Trainings.Remove(training);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TrainingExists(int id)
        {
            return _context.Trainings.Any(e => e.TrainingId == id);
        }
    }
}

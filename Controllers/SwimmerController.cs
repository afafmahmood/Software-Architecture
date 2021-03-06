using CourseProject_SoftwareArchitecture.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CourseProject_SoftwareArchitecture.Controllers
{
    [Authorize(Roles = "Swimmer")]

    public class SwimmerController : Controller
    {
        private readonly ApplicationDbContext db;

        public SwimmerController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index ()
        {
            return View();
        }
        public IActionResult AddProfile()
        {
            var currentUserId = this.User.FindFirst
                (ClaimTypes.NameIdentifier).Value;
            Swimmer swimmer = new Swimmer();
            if (db.Swimmers.Any(i => i.UserId ==
            currentUserId))
            {
                swimmer = db.Swimmers.FirstOrDefault(i =>
                i.UserId == currentUserId);
            }
            else
            {
                swimmer.UserId = currentUserId;
            }
            return View(swimmer);
        }
        [HttpPost]
        public async Task<IActionResult> AddProfile
            (Swimmer swimmer)
        {
            var currentUserId = this.User.FindFirst
                (ClaimTypes.NameIdentifier).Value;
            if(db.Swimmers.Any(i => i.UserId == currentUserId))
            {
                var swimmerToUpdate = db.Swimmers.FirstOrDefault
                    (i => i.UserId == currentUserId);
                swimmerToUpdate.SwimmerId = swimmer.SwimmerId;
                swimmerToUpdate.SwimmerName = swimmer.SwimmerName;
                swimmerToUpdate.Phone = swimmer.Phone;
                swimmerToUpdate.Gender = swimmer.Gender;
                swimmerToUpdate.DateofBirth = swimmer.DateofBirth;
                db.Update(swimmerToUpdate);
            }
            else
            {
                db.Add(swimmer);
            }
            await db.SaveChangesAsync();
            return View("Index");
        }
        public async Task<IActionResult> AllSession()
        {
            var session = await db.Sessions.Include
                (c => c.Coach).ToListAsync();
            return View(session);
        }
        public async Task<IActionResult> EnrollSession(int id)
        {
            var currentUserId = this.User.FindFirst
                (ClaimTypes.NameIdentifier).Value;
            var swimmerId = db.Swimmers.FirstOrDefault
                (s => s.UserId == currentUserId).SwimmerId;
            Enrollment enrollment = new Enrollment
            {
                SessionId = id,
                SwimmerId = swimmerId
            };
            db.Add(enrollment);
            var session = await db.Sessions.FindAsync
                (enrollment.SessionId);
            session.SeatCapacity--;
            await db.SaveChangesAsync();
            return View("Index");
        }
       
        public async Task<IActionResult> CheckReport()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserId = currentUser.FindFirst
                (ClaimTypes.NameIdentifier).Value;
            if (currentUserId == null)
            {
                return NotFound();
            }
            var swimmer = await db.Swimmers
                .SingleOrDefaultAsync
                (s => s.UserId == currentUserId);
            var swimmerId = swimmer.SwimmerId;
            var allSessions = await db.Enrollments
                .Include(e => e.Session).Where
                (C => C.SwimmerId == swimmerId)
                .ToListAsync();
            if (allSessions == null)
            {
                return NotFound();
            }
            ViewData["sname"] = swimmer.SwimmerName;
            return View(allSessions);
        }
    }
}

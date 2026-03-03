using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EF_CoreSync.Data;
using EF_CoreSync.Models;

namespace EF_CoreSync.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context,
                               ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            _logger.LogInformation("Fetching users list");

            var users = _context.Users
                                .AsNoTracking()
                                .OrderByDescending(x => x.CreatedAt)
                                .ToList();

            return View(users);
        }

        // ================= DETAILS =================
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details called with null id");
                return NotFound();
            }

            var user = _context.Users
                               .AsNoTracking()
                               .FirstOrDefault(m => m.Id == id);

            if (user == null)
            {
                _logger.LogWarning("User not found for Details. Id: {Id}", id);
                return NotFound();
            }

            return View(user);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,Email")] User user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create failed due to validation errors");
                return View(user);
            }

            try
            {
                _context.Add(user);
                _context.SaveChanges();

                _logger.LogInformation("User created successfully. Id: {Id}", user.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex,
                    "Duplicate email attempt during Create. Email: {Email}",
                    user.Email);

                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Create");
                return StatusCode(500);
            }
        }

        // ================= EDIT =================
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit called with null id");
                return NotFound();
            }

            var user = _context.Users.Find(id);

            if (user == null)
            {
                _logger.LogWarning("User not found for Edit. Id: {Id}", id);
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name,Email")] User user)
        {
            if (id != user.Id)
            {
                _logger.LogWarning("Edit id mismatch. RouteId: {RouteId}, ModelId: {ModelId}", id, user.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit failed due to validation errors. Id: {Id}", id);
                return View(user);
            }

            try
            {
                var existingUser = _context.Users.Find(id);

                if (existingUser == null)
                {
                    _logger.LogWarning("User not found during update. Id: {Id}", id);
                    return NotFound();
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;

                _context.SaveChanges();

                _logger.LogInformation("User updated successfully. Id: {Id}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex,
                    "Duplicate email during Edit. Email: {Email}",
                    user.Email);

                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Edit. Id: {Id}", id);
                return StatusCode(500);
            }
        }

        // ================= DELETE =================
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete called with null id");
                return NotFound();
            }

            var user = _context.Users
                               .AsNoTracking()
                               .FirstOrDefault(m => m.Id == id);

            if (user == null)
            {
                _logger.LogWarning("User not found for Delete. Id: {Id}", id);
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var user = _context.Users.Find(id);

                if (user == null)
                {
                    _logger.LogWarning("User not found during delete confirmation. Id: {Id}", id);
                    return NotFound();
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                _logger.LogInformation("User deleted successfully. Id: {Id}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Delete. Id: {Id}", id);
                return StatusCode(500);
            }
        }
    }
}
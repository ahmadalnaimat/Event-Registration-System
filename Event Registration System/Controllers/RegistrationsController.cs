using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Event_Registration_System.Data;
using Event_Registration_System.Models;
using Event_Registration_System.Servcies;
using Event_Registration_System.Servcies.Interface;

namespace Event_Registration_System.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly EventContext _context;
        private readonly IEmail _emailService;

        public RegistrationsController(EventContext context, IEmail emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Registrations
        public async Task<IActionResult> Index()
        {
            var eventContext = _context.Registrations.Include(r => r.Event);
            return View(await eventContext.ToListAsync());
        }

        // GET: Registrations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Registrations == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // GET: Registrations/Create
        public IActionResult Create()
        {
            // Populate EventId dropdown for the view
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");
            return View();
        }

        // POST: Registrations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParticipantName,Email,EventId")] Registration registration)
        {
            // Log EventId to see what is being passed
            Console.WriteLine($"EventId: {registration.EventId}");

            if (!ModelState.IsValid)
            {
                // Log the validation errors
                var errors = ModelState.SelectMany(x => x.Value.Errors)
                                       .Select(x => x.ErrorMessage)
                                       .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine(error); // You can replace this with a logger
                }

                // Repopulate ViewData for dropdown in case of validation errors
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", registration.EventId);
                return View(registration);
            }

            // Check if the EventId exists in the database
            if (!_context.Events.Any(e => e.EventId == registration.EventId))
            {
                ModelState.AddModelError("EventId", "The selected event does not exist.");
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", registration.EventId);
                return View(registration);
            }

            // Proceed with registration
            _context.Add(registration);
            await _context.SaveChangesAsync();

            // Fetch the Event manually if needed
            registration.Event = await _context.Events.FindAsync(registration.EventId);

            // Send confirmation email (this part remains the same)
            var subject = "Registration Confirmation";
            var message = $"Dear {registration.ParticipantName},\n\nThank you for registering for the event '{registration.Event.Title}'.";
            await _emailService.SendEmailAsync(registration.Email, subject, message);

            // Redirect to the Index view after successful registration
            return RedirectToAction(nameof(Index));
        }

        // GET: Registrations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Registrations == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", registration.EventId);
            return View(registration);
        }

        // POST: Registrations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParticipantName,Email,EventId")] Registration registration)
        {
            if (id != registration.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationExists(registration.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", registration.EventId);
            return View(registration);
        }

        // GET: Registrations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Registrations == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // POST: Registrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Registrations == null)
            {
                return Problem("Entity set 'EventContext.Registrations' is null.");
            }
            var registration = await _context.Registrations.FindAsync(id);
            if (registration != null)
            {
                _context.Registrations.Remove(registration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegistrationExists(int id)
        {
            return (_context.Registrations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

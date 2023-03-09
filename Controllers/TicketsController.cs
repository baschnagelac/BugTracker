using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Extensions;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums.Enums;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _BTFileService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager,
                                 IBTFileService bTFileService,
                                 IBTTicketService ticketService,
                                 IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _BTFileService = bTFileService;
            _ticketService = ticketService;
            _rolesService = rolesService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId();
            var applicationDbContext = _context.Tickets.Include(t => t.DeveloperUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }


        //GET: My Tickets
        public async Task<IActionResult> MyTicketsIndex()
        {
            int companyId = User.Identity.GetCompanyId();

            string userId = _userManager.GetUserId(User)!;

            List<Ticket> tickets = new List<Ticket>();

            if (User.IsInRole("Submitter"))
            {
                tickets = await _context.Tickets
                                        .Where(c => c.SubmitterUserId == userId)
                                        .Include(t => t.DeveloperUser)
                                        .Include(t => t.Project)
                                        .Include(t => t.TicketPriority)
                                        .Include(t => t.TicketStatus)
                                        .Include(t => t.TicketType)
                                        .ToListAsync();



                return View(tickets);
            }
            else
             if (User.IsInRole("Developer"))
            {
                tickets = await _context.Tickets
                                         .Where(c => c.DeveloperUserId == userId)
                                         .Include(t => t.Project)
                                         .Include(t => t.TicketPriority)
                                         .Include(t => t.TicketStatus)
                                         .Include(t => t.TicketType)
                                         .ToListAsync();


                return View(tickets);

            }
            else
            {
                tickets = await _context.Tickets
                        .Where(c => c.SubmitterUserId == userId)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.Project)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.TicketStatus)
                        .Include(t => t.TicketType)
                        .ToListAsync();

                return View(tickets);
            }

            //return View(new IEnumerable<Ticket>());


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment, int? ticketId)
        {
            ModelState.Remove("UserId");

            if (ModelState.IsValid)

            {
                ticketComment.UserId = _userManager.GetUserId(User);




                ticketComment.Created = DateTime.UtcNow;

                await _ticketService.AddTicketCommentAsync(ticketComment);


            }

            return RedirectToAction("Details", new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;
            ModelState.Remove("TicketId");
            ModelState.Remove("BTUserId");
            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _BTFileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.Now);
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }


        //GET: Assign Project Members 
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignTicketDev(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);


            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);


            string currentDev = ticket.DeveloperUserId!;


            TicketDevViewModel viewModel = new()
            {
                Ticket = ticket,
                DevList = new SelectList(developers, "Id", "FullName", currentDev)

            };

            return View(viewModel);
        }

        //POST: Assign Project Members 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignTicketDev(TicketDevViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket!.Id, companyId);

            if (viewModel.SelectedDev != null)
            {
                //Remove current members
                //await _projectService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, companyId);

                //Add newly selected members 
                await _ticketService.AddDevToTicketAsync(viewModel.SelectedDev, viewModel.Ticket!.Id);

                return RedirectToAction(nameof(Details), new { id = viewModel.Ticket!.Id });


            }

            ModelState.AddModelError("SelectedDev", "No Developer chosen. Please select a developer!");
            // Reset the form 

            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket!.Id, companyId);
            //List<string> currentMembers = viewModel.Ticket.DeveloperUserId.Select(m => m.Id).ToList();
           
            string currentDev = ticket.DeveloperUserId!;

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            viewModel.DevList = new SelectList(developers, "Id", "FullName", currentDev);

            return View(viewModel);
        }


        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                    .ThenInclude(t => t.User)
                .Include(t => t.Attachments)
                .Include(t => t.History)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            int companyId = User.Identity.GetCompanyId();
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {


            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId();

                ticket.SubmitterUserId = _userManager.GetUserId(User);

                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId();
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int companyId = User.Identity.GetCompanyId();
                    ticket.SubmitterUserId = _userManager.GetUserId(User);

                    ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId();
            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            int companyId = User.Identity.GetCompanyId();
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId();
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

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
using System.ComponentModel.Design;
using Org.BouncyCastle.Bcpg;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _BTFileService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTTicketHistoryService _historyService;
        private readonly IBTProjectService _projectService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTCompanyService _companyService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager,
                                 IBTFileService bTFileService,
                                 IBTTicketService ticketService,
                                 IBTRolesService rolesService,
                                 IBTTicketHistoryService historyService, IBTProjectService projectService, IBTNotificationService notificationService, IBTCompanyService companyService)
        {
            _context = context;
            _userManager = userManager;
            _BTFileService = bTFileService;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _historyService = historyService;
            _projectService = projectService;
            _notificationService = notificationService;
            _companyService = companyService;
        }

        // GET: Tickets
        public IActionResult Index()
        {
            int companyId = User.Identity!.GetCompanyId();
            //var applicationDbContext = _context.Tickets
            //                                   .Include(t => t.DeveloperUser)
            //                                   .Include(t => t.Project)
            //                                   .Include(t => t.TicketPriority)
            //                                   .Include(t => t.TicketStatus)
            //                                   .Include(t => t.TicketType);
            //                                   .FirstOrDefaultAsync(t => t.Id == companyId);


            IEnumerable<Ticket> tickets =   _context.Tickets
                                                    .Where(c => c.Archived == false)
                                                    .Where(c => c.Project!.CompanyId == companyId)
                                                          .Include(t => t.DeveloperUser)
                                               .Include(t => t.Project)
                                               
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .ToList();
                                               




            return View(tickets);
        }


        //GET: My Tickets
        public async Task<IActionResult> MyTicketsIndex(int? projectId)
        {
            int companyId = User.Identity.GetCompanyId();

            string userId = _userManager.GetUserId(User)!;

            //BTUser? currentPM = await _projectService.GetProjectManagerAsync(projectId);

            List<Ticket> tickets = new List<Ticket>();

            if (User.IsInRole("Submitter"))
            {
                tickets = await _context.Tickets
                                        .Where(c => c.SubmitterUserId == userId)
                                        .Where(c => c.Archived == false)
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
                                         .Where(c => c.Archived == false)
                                         .Include(t => t.Project)
                                         .Include(t => t.TicketPriority)
                                         .Include(t => t.TicketStatus)
                                         .Include(t => t.TicketType)
                                         .ToListAsync();


                return View(tickets);

            }
            else if (User.IsInRole("ProjectManager"))
            {
                BTUser? user = await _context.Users
                                             .Include(m => m.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                             .Include(m => m.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                             .Include(m => m.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                             .FirstOrDefaultAsync(m => m.Id == userId);

                tickets = user.Projects
                              .SelectMany(p => p.Tickets)
                              .Where(t => t.DeveloperUserId == null)
                              .Where(c => c.Archived == false)
                              .ToList();

                return View(tickets);
            }
            else
            {
                tickets = await _context.Tickets
                        .Where(c => c.SubmitterUserId == userId)
                        .Where(c => c.Archived == false)
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

        //Get Unassigned tickets 
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)},{nameof(BTRoles.Developer)}")]
        public async Task<IActionResult> UnassignedTicketsIndex()
        {

            int companyId = User.Identity.GetCompanyId();

            string userId = _userManager.GetUserId(User)!;


            List<Ticket> tickets = new List<Ticket>();

            //if user is admin > all unassigned
            if (User.IsInRole("Admin"))
            {

                tickets = await _context.Tickets
                                         .Include(t => t.Project)
                                         .Where(c => c.DeveloperUserId == null && c.Project!.CompanyId == companyId)
                                         .Where(c => c.Archived == false)
                                         .Include(t => t.TicketPriority)
                                         .Include(t => t.TicketStatus)
                                         .Include(t => t.TicketType)
                                         .ToListAsync();



            }
            else if (User.IsInRole("ProjectManager"))
            {
                BTUser? user = await _context.Users
                                             .Include(m => m.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                             .Include(m => m.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                             .Include(m => m.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                             .FirstOrDefaultAsync(m => m.Id == userId);

                tickets = user.Projects
                              .SelectMany(p => p.Tickets)
                              .Where(t => t.DeveloperUserId == null)
                              .Where(c => c.Archived == false)
                              .ToList();
            }
            else if (User.IsInRole("Developer"))
            {
                BTUser? user = await _context.Users
                             .Include(m => m.Projects)
                                .ThenInclude(t => t.Tickets)
                                    .ThenInclude(t => t.TicketPriority)
                             .Include(m => m.Projects)
                                .ThenInclude(t => t.Tickets)
                                    .ThenInclude(t => t.TicketStatus)
                             .Include(m => m.Projects)
                                .ThenInclude(t => t.Tickets)
                                    .ThenInclude(t => t.TicketType)
                             .FirstOrDefaultAsync(m => m.Id == userId);

                tickets = user.Projects
                              .SelectMany(p => p.Tickets)
                              .Where(t => t.DeveloperUserId == null)
                              .Where(c => c.Archived == false)
                              .ToList();
            }






            return View(tickets);

        }


        //Get Archived Tickets 
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> TicketArchiveIndex()
        {
            int companyId = User.Identity.GetCompanyId();

            string userId = _userManager.GetUserId(User)!;

            //BTUser? currentPM = await _projectService.GetProjectManagerAsync(projectId);

            List<Ticket> tickets = new List<Ticket>();


            tickets = await _context.Tickets
                                    .Where(c => c.Archived == true)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.Project)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .ToListAsync();



            return View(tickets);


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

                //string userId = _userManager.GetUserId(User)!;

                await _ticketService.AddTicketCommentAsync(ticketComment);




                

                //add history 

                await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);


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


                string userId = _userManager.GetUserId(User)!;

                




                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);


                //add history 

                await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketComment), ticketAttachment.BTUserId);

                statusMessage = "Success: New attachment added to Ticket.";

            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        // GET: TicketAttachments/Delete/5
        //public async Task<IActionResult> DeleteTicketAttachment(int? id)
        //{
        //    if (id == null || _context.TicketAttachments == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketAttachment = await _context.TicketAttachments
        //        .Include(t => t.BTUser)
        //        .Include(t => t.Ticket)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (ticketAttachment == null)
        //    {
        //        return NotFound();
        //    }

        //    return RedirectToAction(nameof(DeleteTicketAttachment), new { id = ticketAttachment.Id });
        //}

        //// POST: TicketAttachments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteTicketAttachmentConfirmed(int id)
        //{
        //    if (_context.TicketAttachments == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.TicketAttachments'  is null.");
        //    }
        //    var ticketAttachment = await _context.TicketAttachments.FindAsync(id);
        //    if (ticketAttachment != null)
        //    {
        //        _context.TicketAttachments.Remove(ticketAttachment);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(DeleteTicketAttachment), new { id = ticketAttachment.Id });
        //}

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

            Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket.Id, companyId);

            Ticket ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket!.Id, companyId);

            string? userId = _userManager.GetUserId(User);



            if (viewModel.SelectedDev != null)
            {
                ticket.DeveloperUserId = viewModel.SelectedDev;
                _context.Update(ticket);
                await _context.SaveChangesAsync();



                await _historyService.AddHistoryAsync(oldTicket, ticket, userId);


                BTUser? btUser = await _userManager.GetUserAsync(User);

                //todo: notfication
                BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);



                Notification? notification = new()
                {
                    TicketId = viewModel.Ticket.Id,
                    Title = "Developer Assigned",
                    ProjectId = ticket.ProjectId,
                    Message = $"New Ticket: {viewModel.Ticket.Title} was assigned by {btUser?.FullName}",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = userId,
                    RecipientId = ticket.DeveloperUserId,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };



                await _notificationService.AddNotificationAsync(notification);
                await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");

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
                .Where(c => c.Project!.CompanyId == companyId)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                    .ThenInclude(t => t.User)
                .Include(t => t.Attachments)
                .Include(t => t.Histories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
  
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User)!;

            Ticket ticket = new();

            await _companyService.GetMemberAsync(companyId);

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");

            ViewData["DeveloperUserId"] = new SelectList(await _companyService.GetMemberAsync(companyId), "Id", "FullName");

            IEnumerable<Project> projects = await _context.Projects
                                                          .Where(p => p.Archived == false && p.CompanyId == companyId)
                                                          .ToListAsync();


            

            //BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket!.ProjectId);


            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            

                

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

                string userId = _userManager.GetUserId(User)!;
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);


                BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);

                //if (userId == ticket.SubmitterUserId || userId == ticket.DeveloperUserId || userId == projectManager.Id)
                //{
                //    
                //}



                //todo: call service
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                //todo: add history record 
                int companyId = User.Identity!.GetCompanyId();



                //Ticket? newTicket = await _context.Tickets
                //                                 .Include(t => t.Project)
                //                                    .ThenInclude(p => p!.Company)
                //                                 .Include(t => t.Attachments)
                //                                 .Include(t => t.Comments)
                //                                 .Include(t => t.DeveloperUser)
                //                                 .Include(t => t.History)
                //                                 .Include(t => t.SubmitterUser)
                //                                 .Include(t => t.TicketPriority)
                //                                 .Include(t => t.TicketStatus)
                //                                 .Include(t => t.TicketType)
                //                                 .AsNoTracking()
                //                                 .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId && t.Archived == false);

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                //call to database
                await _historyService.AddHistoryAsync(null, newTicket, userId);

                BTUser? btUser = await _userManager.GetUserAsync(User);



                Notification? notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket: {ticket.Title} was created by {btUser.FullName}",
                    ProjectId = ticket.ProjectId,
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                if (projectManager != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _notificationService.AdminNotificationAsync(notification, companyId);
                    await _notificationService.SendAdminEmailNotificationAsync(notification, "New Project Ticket Added", companyId);
                }

                
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
            var ticket = await _context.Tickets
               .Where(c => c.Project!.CompanyId == companyId)
               .Include(t => t.DeveloperUser)
               .Include(t => t.Project)
               .Include(t => t.TicketPriority)
               .Include(t => t.TicketStatus)
               .Include(t => t.TicketType)
               .Include(t => t.Comments)
                   .ThenInclude(t => t.User)
               .Include(t => t.Attachments)
               .Include(t => t.Histories)
               .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(await _companyService.GetMemberAsync(companyId), "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);

            IEnumerable<Project> projects = await _context.Projects
                                                         .Where(p => p.Archived == false && p.CompanyId == companyId)
                                                         .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name", ticket.ProjectId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
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
            int companyId = User.Identity.GetCompanyId();
            if (ModelState.IsValid)
            {
                try
                {




                    Ticket? oldTicket = await _context.Tickets
                                                 .Include(t => t.Project)
                                                    .ThenInclude(p => p!.Company)
                                                 .Include(t => t.Attachments)
                                                 .Include(t => t.Comments)
                                                 .Include(t => t.DeveloperUser)
                                                 .Include(t => t.Histories)
                                                 .Include(t => t.SubmitterUser)
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketType)
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId && t.Archived == false);



                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.Now);

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    //call to database
                    string userId = _userManager.GetUserId(User)!;

                    BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);

                    //if (userId == ticket.SubmitterUserId || userId == ticket.DeveloperUserId || userId == projectManager.Id)
                    //{

                    //}





                    //call service
                    Ticket? newTicket = await _context.Tickets
                                                     .Include(t => t.Project)
                                                        .ThenInclude(p => p!.Company)
                                                     .Include(t => t.Attachments)
                                                     .Include(t => t.Comments)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.Histories)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                      .AsNoTracking()
                                                     .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId && t.Archived == false);

                    await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);


                    BTUser? btUser = await _userManager.GetUserAsync(User);


                    Notification? notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = "New Ticket Added",
                        Message = $"New Ticket: {ticket.Title} was created by {btUser.FullName}",
                        ProjectId = ticket.ProjectId,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        SenderId = userId,
                        RecipientId = projectManager?.Id,
                        NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                    };

                    if (projectManager != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                    }
                    else
                    {
                        await _notificationService.AdminNotificationAsync(notification, companyId);
                        await _notificationService.SendAdminEmailNotificationAsync(notification, "New Project Ticket Added", companyId);
                    }


                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction(nameof(Details), new { id = ticket.Id });

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

            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> TicketArchive(int? id)
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

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TicketArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            int companyId = User.Identity.GetCompanyId();



            //var ticket = await _context.Tickets.FindAsync(id);
            var ticket = await _context.Tickets
               .Include(t => t.DeveloperUser)
               .Include(t => t.Project)
               .Include(t => t.TicketPriority)
               .Include(t => t.TicketStatus)
               .Include(t => t.TicketType)
               .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket != null)
            {
                ticket.Archived = true;
                _context.Update(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TicketArchiveIndex));
        }



        // GET: Tickets/Restore/5
        public async Task<IActionResult> TicketRestore(int? id)
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

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TicketRestoreConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            int companyId = User.Identity.GetCompanyId();



            //var ticket = await _context.Tickets.FindAsync(id);
            var ticket = await _context.Tickets
               .Include(t => t.DeveloperUser)
               .Include(t => t.Project)
               .Include(t => t.TicketPriority)
               .Include(t => t.TicketStatus)
               .Include(t => t.TicketType)
               .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket != null)
            {
                ticket.Archived = false;
                _context.Update(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId();
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }












        ////get tickets archive 
        //[HttpGet]
        //public async Task<IActionResult> TicketArchive(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticket = await _ticketService.GetTicketsAsync(id.Value);

        //    if (ticket == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(ticket);
        //}

        ////post tickets archive 
        //[HttpPost]
        //public async Task<IActionResult> TicketArchive(Ticket ticket)
        //{
        //    ticket.Archived = true;
        //    _context.Update(ticket);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(TicketArchive));


        //}

        //public IActionResult ArchiveCheckT()
        //{
        //    return View();
        //}


    }
}

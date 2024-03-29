﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Extensions;
using BugTracker.Models.ViewModels;
using BugTracker.Models.Enums.Enums;
using System.Collections;
using BugTracker.Services;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _btFileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTFileService bTFileService, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _btFileService = bTFileService;
            _projectService = projectService;
            _rolesService = rolesService;
        }


        //GET:
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get companyId
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);
            //string? firstName = currentPM.FirstName;

            //string fullName = currentPM?.FullName ?? "Unassigned";
            //string fullNameV2 = (await _projectService.GetProjectManagerAsync(id))?.FullName ?? "Unassigned";

            /*
             <html>
            @{
                BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);
                string fullName = currentPM?.FullName ?? "Unassigned";
            }
            <p>@fullName</p>
            <p>@( (await _projectService.GetProjectManagerAsync(id))?.FullName ?? "Unassigned" ) </p>
            </html>
             
             */


            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id

            };
            return View(viewModel);
        }

        //POST:
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if(!string.IsNullOrEmpty(viewModel.PMId))
            {
                await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project?.Id);

                return RedirectToAction("Details", new { id = viewModel.Project?.Id });
            }

            ModelState.AddModelError("PMId", "No Project Manager chosen. Please select a PM.");

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.Project?.Id);

            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project?.Id, companyId);

            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);

            viewModel.PMId = currentPM?.Id;

            return View(viewModel);
        }

        //GET: Assign Project Members 
        [HttpGet]
        [Authorize(Roles ="Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            List<BTUser> userList = submitters.Concat(developers).ToList();

            List<string> currentMembers = project.Members.Select(m => m.Id).ToList();


            ProjectMembersViewModel viewModel = new()
            {
                Project = project,
                UsersList = new MultiSelectList(userList, "Id", "FullName", currentMembers)

            };

            return View(viewModel);
        }

        //POST: Assign Project Members 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (viewModel.SelectedMembers != null)
            {
                //Remove current members
                await _projectService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, companyId);

                //Add newly selected members 
                await _projectService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel.Project!.Id, companyId);

                return RedirectToAction(nameof(Details), new { id = viewModel.Project!.Id });


            }

            ModelState.AddModelError("SelectedMembers", "No Users chosen. Please select Users!");
            // Reset the form 

            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project!.Id, companyId);
            List<string> currentMembers = viewModel.Project.Members.Select(m => m.Id).ToList();

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> userList = submitters.Concat(developers).ToList();

            viewModel.UsersList = new MultiSelectList(userList, "Id", "FullName", currentMembers);

            return View(viewModel); 
        }




        // GET: Projects
        public async Task<IActionResult> Index(Project project)
        {


            int companyId = User.Identity!.GetCompanyId();

            if (project.ImageFormFile != null)
            {
                project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                project.ImageFileType = project.ImageFormFile.ContentType;
            }

            //IEnumerable<Project> projects = await _context.Projects
            //                                              .Where(p => p.Archived == false && p.CompanyId == companyId)
            //                                              .Include(p => p.Members)
            //                                              .Include(p => p.Members)
            //                                              .Include(p => p.ProjectPriority)
            //                                              .Include(p => p.Tickets)
            //                                              .ToListAsync();

            IEnumerable<Project> projects = await _projectService.GetProjectsAsync(companyId);

            return View(projects);

        }

        //GET: My Projects
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyProjectsIndex(int? ticketId)
        {
            //BTUser? btUser = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId();

            string userId = _userManager.GetUserId(User)!;


            List<Project> projects = new List<Project>();
            List<BTUser> users = new List<BTUser>();


            projects = await _context.Projects
                                     .Where(p => p.Members.Any(users => users.Id == userId))
                                     .Where(c => c.Archived == false)
                                     .Include(p => p.Company)
                                     .Include(p => p.ProjectPriority)
                                     .ToListAsync();



            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(projects);

        }



        //get tickets archive 
        //[HttpGet]
        //public async Task<IActionResult> ProjectArchive(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var project = (await _projectService.GetAllProjectsAsync(id.Value)).Where(p=>p.Archived == true);
        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(project);
        //}

    





        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();


            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


            if (project == null)
            {
                return NotFound();
            }

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Create
       
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId();
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(new Project());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create([Bind("Id,BTUserId,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFormFile,ImageFileData,ImageFileType,Archived")] Project project)
        {
            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId();

                BTUser? btUser = await _userManager.GetUserAsync(User);

                project.CompanyId = btUser!.CompanyId;

                //format date(s)
                project.Created = DataUtility.GetPostGresDate(DateTime.Now);


                project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                project.EndDate = DataUtility.GetPostGresDate(project.EndDate);


                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }




                _context.Add(project);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            //ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();

            var project = await _context.Projects.FindAsync(id);


            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int companyId = User.Identity.GetCompanyId();

                    string? userId = _userManager.GetUserId(User);

                    //reformat created date
                    project.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);



                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }


                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> ProjectArchive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            int companyId = User.Identity.GetCompanyId();

            //var project = await _context.Projects.FindAsync(id);
            var project = await _context.Projects
                                        .Include(p => p.Company)
                                        .Include(p => p.ProjectPriority)
                                        .FirstOrDefaultAsync(m => m.Id == id);


            if (project != null)
            {
                project.Archived = true;
                _context.Update(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProjectArchiveIndex));
        }

        //Get Archived Projects
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> ProjectArchiveIndex()
        {
            int companyId = User.Identity.GetCompanyId();

            string userId = _userManager.GetUserId(User)!;

            //BTUser? currentPM = await _projectService.GetProjectManagerAsync(projectId);

            List<Project> projects = new List<Project>();


            projects = await _context.Projects
                                    .Where(c => c.Archived == true)
                                    .Include(p => p.Company)
                                     .Include(p => p.ProjectPriority)
                                    .ToListAsync();



            return View(projects);


        }

        // GET: Projects/Restore/5
        public async Task<IActionResult> ProjectRestore(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectRestoreConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            int companyId = User.Identity.GetCompanyId();

            //var project = await _context.Projects.FindAsync(id);
            var project = await _context.Projects
                                        .Include(p => p.Company)
                                        .Include(p => p.ProjectPriority)
                                        .FirstOrDefaultAsync(m => m.Id == id);


            if (project != null)
            {
                project.Archived = false;
                _context.Update(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

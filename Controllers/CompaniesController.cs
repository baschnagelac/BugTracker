using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BugTracker.Models.ViewModels;
using BugTracker.Extensions;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums.Enums;
using BugTracker.Services;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;
        private readonly UserManager<BTUser> _userManager;

        public CompaniesController(ApplicationDbContext context, IBTRolesService rolesService, IBTCompanyService companyService, UserManager<BTUser> userManager)
        {
            _context = context;
            _rolesService = rolesService;
            _companyService = companyService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            //1 add an instance of viewmodel as a list (model)
            List<ManageUserRolesViewModel> viewModel = new()
            {

            };

            //2 get companyId
            int companyId = User.Identity!.GetCompanyId();

            //3 get all company users 
            List<BTUser> companyUsers = await _context.Users
                                                .Where(u => u.CompanyId == companyId).ToListAsync();

            List<IdentityRole> roles = await _rolesService.GetRolesAsync();

            

          


            //4 loop over the users to populate the instance of the ViewModel
            //          - instantiatew single viewmodel,
            //          use _roleservice tp help populate the viewmodel instance
            //           instantiate and assign the multiselect,
            //            add viewmodel to model List

            foreach (BTUser companyUser in companyUsers)
            {
                List<string> result = (await _rolesService.GetUserRolesAsync(companyUser)).ToList();

                ManageUserRolesViewModel model = new()
                {
                   
                    BTUser = companyUser,
                    
                    Roles = new MultiSelectList(roles, "Name", "Name", result),
                    
                    SelectedRoles = result


                };
                viewModel.Add(model);
            }
            //5 return the model to the view 
            return View(viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            //1 get the company Id 
            int companyId = User.Identity!.GetCompanyId();

            //2 instantiate the BTUser
            BTUser? btUser = await _userManager.FindByIdAsync(viewModel.BTUser!.Id);

            if (btUser == null)
            {
                return NotFound();
            }




            //3 get roles for the user 

            List<string> currentRoles = (await _rolesService.GetUserRolesAsync(btUser)).ToList();


            ////4 get selected role(s) for the user submitted from the form 
            //await _rolesService.GetRolesAsync();

            ////5 remove current role(s) and Add new role 
            //await _rolesService.RemoveUserFromRolesAsync(viewModel.BTUser,  viewModel.SelectedRoles);

            await _rolesService.RemoveUserFromRolesAsync(btUser, currentRoles);

            foreach (string role in viewModel.SelectedRoles)
            {
                await _rolesService.AddUserToRoleAsync(btUser, role);
            }
            

            //6 navigate 
            return RedirectToAction("Index");
        }

        // GET: Companies
        public IActionResult Index()
        {


            int companyId = User.Identity!.GetCompanyId();

            return RedirectToAction(nameof(Details), new { companyId });
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

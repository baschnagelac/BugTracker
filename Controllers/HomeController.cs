using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTTicketService _ticketService;

        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTCompanyService companyService, IBTTicketService ticketService)
        {
            _logger = logger;
            _projectService = projectService;
            _companyService = companyService;
            _ticketService = ticketService; 
        }

        public IActionResult Index()
        {

            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View();
            
        }


        public async Task<IActionResult> Dashboard(DashboardViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            await _companyService.GetCompanyInfoAsync(companyId);

            await _projectService.GetProjectsAsync(companyId);

            await _ticketService.GetTicketsAsync(companyId);

            await _companyService.GetMemberAsync(companyId);

           

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
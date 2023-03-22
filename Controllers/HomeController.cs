using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ChartModels;
using BugTracker.Models.Enums.Enums;
using BugTracker.Models.ViewModels;
using BugTracker.Services;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BugTracker.Controllers
{
    [Authorize]
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

        [AllowAnonymous] 
        public IActionResult Index()
        {

            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View();

        }


        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel viewModel = new();


            int companyId = User.Identity!.GetCompanyId();



            viewModel.Company = await _companyService.GetCompanyInfoAsync(companyId);

            viewModel.Projects = (await _projectService.GetAllProjectsAsync(companyId)).ToList();

            viewModel.Tickets = (await _ticketService.GetTicketsAsync(companyId)).ToList();

            viewModel.Members = await _companyService.GetMemberAsync(companyId);



            return View(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _projectService.GetAllProjectsByCompanyAsync(companyId)).ToList();

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name!, prj.Tickets.Count() });
            }

            return Json(chartData);
        }


        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _projectService.GetAllProjectsByCompanyAsync(companyId)).ToList();

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }


        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {

            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _projectService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();

            foreach (Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name!;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _projectService.GetAllProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Developer))).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();


            return Json(amChartData.Data);
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
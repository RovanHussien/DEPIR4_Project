using DEPI.DAL.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DEPI_Pro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<IdentityRole> _rolemanager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, RoleManager<IdentityRole> rolemanager,ApplicationDbContext context)
        {
            _logger = logger;
            _rolemanager = rolemanager;
            _context = context;
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View("RoleForm");
        }
        //[HttpPost]
        //public async Task<IActionResult> CreateRole(RoleViewModel roleView)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        IdentityRole identityRole = new IdentityRole
        //        {
        //            Name = roleView.Name
        //        };
        //        var result = await _rolemanager.CreateAsync(identityRole);
        //        if(result.Succeeded)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            foreach(var item in result.Errors)
        //            {
        //                ModelState.AddModelError("", item.Description);
        //            }
        //        }
        //    }
        //    return View("RoleForm", roleView);
        //}
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        
    }
}

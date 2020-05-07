using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mock_sns_core2.Models;

namespace mock_sns_core2.Controllers
{
    [Route("/Home")]
    [Route("")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Route("")]
        [Route("Index")]
        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.ApplicationUserName = user.ApplicationUserName;

            string sql = "select * from Article " +
                " inner join \"AspNetUsers\" " +
                " On Article.UserId = \"AspNetUsers\".\"Id\" " +
                " where Article.UserId = :UserId " +
                " order by PostDate desc";
            ViewBag.list = await new Article().search(sql, new { UserId = user.Id });
            
            return View("Index");
        }

        [Route("UserPhoto/{UserName}")]
        public async Task<IActionResult> DispUserPhotoAsync(string UserName)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(UserName);

            return new FileContentResult(user.Photo, "image/photo");
        }

        [Route("About")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Route("Contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

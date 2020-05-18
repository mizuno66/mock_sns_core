using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mock_sns_core2.Models;
using mock_sns_core2.Services;

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
        public async Task<IActionResult> IndexAsync(int? pageNum)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.ApplicationUserName = user.ApplicationUserName;

            ViewData["pageNum"] = pageNum ?? 1;

            string sql = "select * from Article " +
                " inner join \"AspNetUsers\" " +
                " On Article.UserId = \"AspNetUsers\".\"Id\" " +
                " where Article.UserId = :UserId " +
                " order by PostDate desc";

            var list = await new Article().search(sql, new { UserId = user.Id });
            ViewBag.list = PaginatedService<Article>.Create(list, pageNum ?? 1, 8);
            
            return View("Index");
        }

        [Route("UserPhoto/{UserName}")]
        public async Task<IActionResult> DispUserPhotoAsync(string UserName)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(UserName);

            return new FileContentResult(user.Photo, "image/jpeg");
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

        [Route("UploadPhysical")]
        [HttpPost]
        public async Task<IActionResult> UploadPhysical(List<IFormFile> files, string messageInput, string userInput)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(userInput);

            var art = new Article();
            art.User = user;
            art.PostDate = DateTime.Now;
            art.Text = messageInput;
            var result = await art.insert();

            foreach (var formFile in files)
            {
                if(formFile.Length > 0)
                {
                    var filename = Path.GetRandomFileName();
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), Startup.StoredFilePath, filename);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            return Ok(new { art_Id = art.Id });
        }
    }
}

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

            string sql = " where Article.UserId = :UserId " +
                " order by PostDate desc";

            var dbcs = new DbConnectionService();
            dbcs.Open();
            var list = await new Article().search(dbcs, sql, new { UserId = user.Id });
            dbcs.Close();

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

        [Route("Content/{UserName}/{FileName}")]
        public async Task<IActionResult> DispContentsAsync(string UserName, string fileName)
        {
            byte[] data = await System.IO.File.ReadAllBytesAsync(Path.Combine(Startup.StoredFilePath, fileName));
            return new FileContentResult(data, "image/jpeg");
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
        public async Task<IActionResult> UploadPhysical(List<IFormFile> imagefiles, string messageInput, string userInput)
        {
            var dbcs = new DbConnectionService();
            dbcs.Open();
            dbcs.BeginTran();

            // ユーザ情報取得
            var user = new ApplicationUser();
            user = await user.getUserAsync(userInput);          

            var art = new Article();
            art.User = user;
            art.PostDate = DateTime.Now;
            art.Text = messageInput;

            var contents = new List<ArticleContents>();
            // 画像ファイル
            foreach (var formFile in imagefiles)
            {
                var artC = new ArticleContents();

                if (formFile.Length > 0 && formFile.Length < 5242880)
                {
                    var filename = Path.GetRandomFileName();
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), Startup.StoredFilePath, filename);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);

                        stream.Seek(0, SeekOrigin.Begin);
                        using(var reader = new BinaryReader(stream))
                        {
                            artC.ContentType = FileSignatureService.checkImageSignature(reader.ReadBytes(FileSignatureService.MaxImageBytes));
                        }
                    }
                    artC.FileName = filename;

                    if(artC.ContentType == "")
                    {
                        return BadRequest(Json("Error:Not Match ContentType"));
                    }
                    contents.Add(artC);
                }
                else
                {
                    return BadRequest("Error:Maximum Image File Size is 5MB");
                }
            }

            art.Contents = contents;
            try
            {
                var result = await art.insert(dbcs);
                if (result >= 1)
                {
                    dbcs.CommitTran();
                }
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex)
            {
                Console.WriteLine(ex);
                dbcs.RollbackTran();
                return BadRequest("Error:DB Access");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
                dbcs.RollbackTran();
                return BadRequest("Error:Invalid Operation");
            }
            finally
            {
                dbcs?.Close();   
            }

            return Ok(new { art_Id = art.Id });
        }
    }
}

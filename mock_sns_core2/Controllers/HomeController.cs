using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private IHostingEnvironment _hostingEnviroment;

        public HomeController(UserManager<ApplicationUser> userManager, IHostingEnvironment environment)
        {
            _userManager = userManager;
            _hostingEnviroment = environment;
        }

        [Route("")]
        [Route("Index")]
        [Authorize]
        public async Task<IActionResult> IndexAsync(int? pageNum)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.ApplicationUserName = user.ApplicationUserName;

            ViewData["pageNum"] = pageNum ?? 1;

            string sql = " where UserId In (select FollowUserId from FollowUsers Where UserId = :UserId) " +
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

        //[Route("Content/Photo/{UserName}/{FileName}")]
        //public async Task<IActionResult> DispPhotoAsync(string UserName, string fileName)
        //{
        //    byte[] data = await System.IO.File.ReadAllBytesAsync(Path.Combine(Startup.StoredFilePath, UserName, fileName));
        //    return new FileContentResult(data, "image/jpeg");
        //}

        //[Route("Content/Video/{UserName}/{FileName}")]
        //public async Task<IActionResult> DispVideoAsync(string UserName, string fileName)
        //{
        //    byte[] data = await System.IO.File.ReadAllBytesAsync(Path.Combine(Startup.StoredFilePath, UserName, fileName, "video.m3u8"));
        //    return new FileContentResult(data, "application/vnd.apple.mpegurl");
        //}

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
        [Authorize]
        public async Task<IActionResult> UploadPhysical(List<IFormFile> imagefiles,List<IFormFile> videofiles, string messageInput, string userInput)
        {
            var dbcs = new DbConnectionService();
            dbcs.Open();
            dbcs.BeginTran();

            // ユーザ情報取得
            var user = new ApplicationUser();
            user = await user.getUserAsync(userInput);          

            if(messageInput == null && imagefiles.Count == 0 && videofiles.Count == 0)
            {
                return BadRequest(Json("Error:入力または、選択をしてください。"));
            }
            var art = new Article();
            art.User = user;
            art.PostDate = DateTime.Now;
            art.Text = messageInput;

            // フォルダ作成
            var contentDir = Path.Combine(_hostingEnviroment.WebRootPath, Startup.StoredFilePath, user.UserName);
            if (!Directory.Exists(contentDir))
            {
                Directory.CreateDirectory(contentDir);
            }

            var contents = new List<ArticleContents>();
            // 画像ファイル
            foreach (var formFile in imagefiles)
            {
                var artC = new ArticleContents();

                if (formFile.Length > 0 && formFile.Length < 5242880)
                {
                    var filename = Path.GetRandomFileName();
                    var filePath = Path.Combine(contentDir, filename);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);

                        stream.Seek(0, SeekOrigin.Begin);
                        using(var reader = new BinaryReader(stream))
                        {
                            artC.ContentType = FileSignatureService.checkImageSignature(reader.ReadBytes(FileSignatureService.MaxImageBytes));
                        }
                    }

                    if(artC.ContentType == "")
                    {
                        return BadRequest(Json("Error:Not Match ContentType"));
                    }

                    System.IO.File.Move(filePath, filePath + artC.getExtension());
                    artC.FileName = filename + artC.getExtension();
                    
                    contents.Add(artC);
                }
                else
                {
                    return BadRequest(Json("Error:Maximum Image File Size is 5MB"));
                }
            }

            // 動画ファイル
            foreach(var formFile in videofiles)
            {
                var artC = new ArticleContents();

                if(formFile.Length > 0 && formFile.Length < 536870912)
                {
                    var filename = Path.GetRandomFileName();
                    // ファイル名のディレクトリを作成
                    Directory.CreateDirectory(Path.Combine(contentDir, filename));
                    var filePath = Path.Combine(contentDir,filename, filename);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);

                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new BinaryReader(stream))
                        {
                            artC.ContentType = FileSignatureService.checkVideoSignature(reader.ReadBytes(FileSignatureService.MaxVideoBytes));
                        }
                    }

                    HLSConvertService.convertHLS(filePath);
                    HLSConvertService.createThumbnail(filePath);

                    artC.FileName = filename;

                    if (artC.ContentType == "")
                    {
                        return BadRequest(Json("Error:Not Match ContentType"));
                    }
                    contents.Add(artC);
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
                return BadRequest(Json("Error:DB Access"));
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
                dbcs.RollbackTran();
                return BadRequest(Json("Error:Invalid Operation"));
            }
            finally
            {
                dbcs?.Close();   
            }

            return Ok(new { art_Id = art.Id });
        }
    }
}

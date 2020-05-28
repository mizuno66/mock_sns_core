using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mock_sns_core2.Models;
using mock_sns_core2.Services;
using Oracle.ManagedDataAccess.Client;

namespace mock_sns_core2.Controllers
{
    [Route("/User")]
    public class UserController : Controller
    {
        public UserController()
        {
        }

        [Route("{UserName}")]
        public async Task<IActionResult> Index(string UserName, int? pageNum)
        {
            var DispUser = new ApplicationUser();
            DispUser = await DispUser.getUserAsync(UserName);

            ViewBag.user = DispUser;
            ViewData["pageNum"] = pageNum ?? 1;

            var dbcs = new DbConnectionService();
            dbcs.Open();
            // ログインしている場合、ログインユーザのマイページかとフォロー状態をチェック
            if (User.Identity.IsAuthenticated)
            {
                var LoginUser = new ApplicationUser();
                LoginUser = await LoginUser.getUserAsync(User.Identity.Name);

                if(LoginUser.Id == DispUser.Id)
                {
                    ViewBag.mypage = true;
                    ViewBag.follow = "false";
                }
                else
                {
                    ViewBag.mypage = false;
                    ViewBag.follow = await FollowUsers.checkFollow(dbcs, LoginUser.Id, DispUser.Id) ;
                }
            }

            string sql = " where Article.UserId = :UserId " +
                " order by PostDate desc";
            
            var list = await new Article().search(dbcs, sql, new { UserId = DispUser.Id });
            dbcs.Close();

            ViewBag.list = PaginatedService<Article>.Create(list, pageNum ?? 1, 8);

            return View("Index");
        }

        [Route("FollowUser")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FollowUser(string userName, string isFollow)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(userName);

            var loginUser = new ApplicationUser();
            loginUser = await loginUser.getUserAsync(User.Identity.Name);

            var dbcs = new DbConnectionService();
            try
            {
                dbcs.Open();
                dbcs.BeginTran();

                var follow = new FollowUsers(loginUser, user);
                if (isFollow == "true")
                {
                    // フォロー解除
                    await follow.delete(dbcs);
                    isFollow = "false";
                }
                else
                {
                    //フォロー
                    await follow.insert(dbcs);
                    isFollow = "true";
                }
                dbcs.CommitTran();
            }
            catch(OracleException ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Network Error");
            }
            finally
            {
                dbcs.Close();
            }
            
            return Ok(new { isFollow = isFollow });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using mock_sns_core2.Models;
using mock_sns_core2.Services;

namespace mock_sns_core2.Hubs
{
    public class ArticleHub : Hub
    {
        public ArticleHub()
        {
        }

        public override async Task OnConnectedAsync()
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(Context.User.Identity.Name);

            var dbcs = new DbConnectionService();
            dbcs.Open();
            var fu = new FollowUsers();
            var list = await fu.GetListAsync(dbcs, user.Id);

            List<Task> tasks = new List<Task>();
            tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, user.UserName));

            foreach(var f in list)
            {
                tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, f.FollowUser.UserName));
            }
            await Task.WhenAll(tasks);

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string art_Id, string userName, string message)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(userName);

            var artC = new ArticleContents();
            var cList = await artC.GetListAsync(long.Parse(art_Id));

            var imageContents = "";
            imageContents = string.Join<string>(",", cList?.Where(c => c.getContentType() == "image").Select(c => c.FileName));

            var videoContents = "";
            videoContents = string.Join<string>(",", cList?.Where(c => c.getContentType() == "video").Select(c => c.FileName));

            await Clients.Group(userName).SendAsync("ReceiveMessage", art_Id, userName, user.ApplicationUserName, message, imageContents, videoContents);
        }
    }
}

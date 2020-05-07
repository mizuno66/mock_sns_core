using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using mock_sns_core2.Models;

namespace mock_sns_core2.Hubs
{
    public class ArticleHub : Hub
    {
        public ArticleHub()
        {
        }

        public async Task SendMessage(string userName, string message)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(userName);

            var art = new Article();
            art.User = user;
            art.PostDate = DateTime.Now;
            art.Text = message;
            var result = await art.insert();

            if(result >= 1)
            {
                await Clients.All.SendAsync("ReceiveMessage", art.Id, userName, user.ApplicationUserName, message);
            }
        }
    }
}

using System;
using System.Linq;
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

        public async Task SendMessage(string art_Id, string userName, string message)
        {
            var user = new ApplicationUser();
            user = await user.getUserAsync(userName);

            var artC = new ArticleContents();
            var cList = await artC.GetListAsync(long.Parse(art_Id));

            var contents = "";
            contents = string.Join<string>(",", cList?.Select(c => c.FileName));

            await Clients.All.SendAsync("ReceiveMessage", art_Id, userName, user.ApplicationUserName, message, contents);
        }
    }
}

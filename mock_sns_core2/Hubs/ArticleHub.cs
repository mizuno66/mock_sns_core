using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace mock_sns_core2.Hubs
{
    public class ArticleHub : Hub
    {
        public ArticleHub()
        {
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace chatBackDude
{
    public class ChatHub: Hub
    {
        public async Task JoinChat(string user)
    {
        await Clients.All.SendAsync("JoinChat", user);
    }

    public async Task LeaveChat(string user)
    {
        await Clients.All.SendAsync("LeaveChat", user);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

}
}

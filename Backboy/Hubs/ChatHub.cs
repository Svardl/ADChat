using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Backboy.Hubs {
    public class ChatHub : Hub {

        static List<Message> Msg = new List<Message>();
        static List<Message> Events = new List<Message>();
        public async Task JoinChat(string user, string connectionID) {
            await Clients.All.SendAsync("JoinChat", user);
            await Clients.Caller.SendAsync("ChatHistory", Msg);
        }

        public async Task LeaveChat(string user) {
            await Clients.All.SendAsync("LeaveChat", user);
        }

        public async Task SendMessage(string user, string message, int r, int g, int b) {
            await Clients.All.SendAsync("ReceiveMessage", user, message, r,g,b);
            Msg.Add(new Message() { author=user, message=message, R=r, G=g, B=b,} );
        }

        public async Task AddEvent(string user, string message, DateTime dt) {
            Message eventboy = new Message() { author = user, message = message, time = dt };
            await Clients.All.SendAsync("AddEvent", eventboy);
        }

    }

    public class Message {
        public string author { get; set; }
        public string message { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public DateTime time { get; set; }
    }
}

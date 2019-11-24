using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Backboy.Hubs
{
    public class ChatHub : Hub
    {

        static List<Message> Msg = new List<Message>();
        static List<Message> Events = new List<Message>();
        static List<Message> Online = new List<Message>();

        public async Task JoinChat(string user) {

            Events.Clear(); // remove in release
            string connectionID = Context.ConnectionId;

            Message newEntry = new Message() { message = connectionID, author = user, time = DateTime.Now };
            if (Online.Where(person => person.author.Equals(user)).Count() == 0) {
                Online.Add(newEntry);
            }
            else {
                Online.FirstOrDefault(person => person.author.Equals(user)).message = connectionID;
                Online.FirstOrDefault(person => person.author.Equals(user)).time = DateTime.Now;

            }

            await Clients.All.SendAsync("JoinChat", user);
            await Clients.Caller.SendAsync("ChatHistory", Msg);
            await Clients.All.SendAsync("activeUserList", Online, true, newEntry);
        }

        public override async Task OnDisconnectedAsync(Exception ex) {

            var connectionID = Context.ConnectionId;

            var user = Online.FirstOrDefault(person => person.message.Equals(connectionID)).author;
            Message newEntry = new Message() { message = connectionID, author = user, time = DateTime.Now };

            await Clients.All.SendAsync("activeUserList", Online, false, newEntry);

            Online.Remove(Online.FirstOrDefault(person => person.message.Equals(connectionID)));
           
            await base.OnDisconnectedAsync(ex);
        
        }

        public async Task LeaveChat(string user) {
            await Clients.All.SendAsync("LeaveChat", user);
        }

        public async Task SendMessage(string user, string message, int r, int g, int b) {
            await Clients.All.SendAsync("ReceiveMessage", user, message, r,g,b);
            Msg.Add(new Message() {author=user, message=message, R=r, G=g, B=b,} );
        }

        public async Task AddEvent(string user, string message, DateTime dt) {
            Message eventboy = new Message() {author = user, message = message, time = dt };
            int ind = insertDate(eventboy);
            Events.Insert(ind, eventboy);
            await Clients.All.SendAsync("AddEvent", eventboy, ind);
        }


        private int insertDate(Message CurrEv) {
            int i = 0;
            foreach (Message ev in Events) {
                if (DateTime.Compare(CurrEv.time, ev.time) < 0) {
                    return i;
                }
                i++;
            }
            return i;
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

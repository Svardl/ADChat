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
        static int EventId = 0;
        static Dictionary<int, List<string>> attending = new Dictionary<int, List<string>>();
        static Dictionary<int, List<string>> declined = new Dictionary<int, List<string>>();

        public async Task JoinChat(string user) {

            //Events.Clear(); // remove in release
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
            clearPassedEvents();

            await Clients.Caller.SendAsync("EventHistory", Events);
            foreach (int id in attending.Keys) {
                await Clients.Caller.SendAsync("UpdateAttendingEvent", attending[id], declined[id], id);
            }
        }

        private void clearPassedEvents() {
            for (int i = Events.Count-1; i>0; i--) {
                if (Events[i].time.CompareTo(DateTime.Now.AddHours(3)) > 1) {
                    Events.Remove(Events[i]);
                    attending.Remove(Events[i].R);
                    declined.Remove(Events[i].R);
                }
            }

        }
        public override async Task OnDisconnectedAsync(Exception ex) {

            var connectionID = Context.ConnectionId;

            var user = Online.FirstOrDefault(person => person.message.Equals(connectionID)).author;
            Message newEntry = new Message() { message = connectionID, author = user, time = DateTime.Now };

            Online.Remove(Online.FirstOrDefault(person => person.message.Equals(connectionID)));

            await Clients.All.SendAsync("activeUserList", Online, false, newEntry);
            await base.OnDisconnectedAsync(ex);
        
        }

        public async Task LeaveChat(string user) {

            Message newEntry = new Message() { message = null, author = user, time = DateTime.Now };
            Online.Remove(Online.FirstOrDefault(person => person.author.Equals(user)));

            await Clients.All.SendAsync("activeUserList", Online, false, newEntry);
            await Clients.All.SendAsync("LeaveChat", user);
        }

        public async Task SendMessage(string user, string message, int r, int g, int b) {
            await Clients.All.SendAsync("ReceiveMessage", user, message, r,g,b);
            Msg.Add(new Message() {author=user, message=message, R=r, G=g, B=b,} );
        }

        public async Task AddEvent(string user, string message, DateTime dt) {
            Message eventboy = new Message() {author = user, message = message, time = dt, R=EventId };
            
            int ind = insertDate(eventboy);
            Events.Insert(ind, eventboy);
            attending.Add(EventId, new List<string>());
            declined.Add(EventId, new List<string>());
            EventId++;
            await Clients.All.SendAsync("AddEvent", eventboy, ind);
        }

        public async Task RemoveEvent(int eventId) {

            var removed = Events.FirstOrDefault(ev =>ev.R == eventId);

            if (removed != null) {
                Events.Remove(removed);
                attending.Remove(eventId);
                declined.Remove(eventId);
                await Clients.All.SendAsync("RemoveEvent", eventId);
            }
        }

        public async Task AttendEvent(string user, int eventId) {

            if (declined.ContainsKey(eventId) && declined[eventId].Any(u => u.Equals(user))) {
            declined[eventId].Remove(declined[eventId].First(u => u.Equals(user)));   
            }
            List<string> attreturn = null;
            List<string> decreturn = declined.ContainsKey(eventId) ? declined[eventId] : null;

            if (attending.ContainsKey(eventId)) {
                attending[eventId].Add(user);
                attreturn = attending[eventId];
            }
            await Clients.All.SendAsync("UpdateAttendingEvent", attreturn, decreturn, eventId);
        }

        public async Task DeclineEvent(string user, int eventId) {
            if (attending.ContainsKey(eventId) && attending[eventId].Any(u => u.Equals(user))) {
                attending[eventId].Remove(attending[eventId].First(u => u.Equals(user)));
            }
            List<string> decreturn = null;
            List<string> attreturn = attending.ContainsKey(eventId) ? attending[eventId] : null;

            if (declined.ContainsKey(eventId)) {
                declined[eventId].Add(user);
                decreturn = declined[eventId];
            }
            await Clients.All.SendAsync("UpdateAttendingEvent", attreturn, decreturn, eventId);
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

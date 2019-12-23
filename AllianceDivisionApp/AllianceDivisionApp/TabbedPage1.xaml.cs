using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.SimpleAudioPlayer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using System.Collections.ObjectModel;
using Rg.Plugins.Popup.Services;

namespace AllianceDivisionApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage {
        ISimpleAudioPlayer player;

        bool alreadyLoaded = false;
        string name;
        HubConnection hubConnection;
        bool isConnected = false;
        System.Drawing.Color cellColor;
        //public IList<string> activeUsers { get; set; }
        public ObservableCollection<OnlineUser> ObActiveUsers = new ObservableCollection<OnlineUser>();
        PopUpForm popupRef;
        List<Message> EventsList = new List<Message>();
        TwoWay twoWay = new TwoWay();
        int LastId;
        EventPage LastVisitedEvent;
        Dictionary<int, List<string>> attending = new Dictionary<int, List<string>>();
        Dictionary<int, List<string>> declined = new Dictionary<int, List<string>>();


        public TabbedPage1(string name) {
            InitializeComponent();

            var clr = Color.FromHex("#4A5B64");
            this.BarBackgroundColor = clr;
            this.BindingContext = this;
            
            player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();

            CurrentPage = Children[1];

            Children[0].Appearing += UpdateOnline;
            Send.Clicked += Send_Clicked;
            this.name = name;
            Random rand = new Random();

            cellColor = GenColor(); 
                
            ConnectsBtn.Clicked += toggleBtn;
           
           //AddBtn.Clicked += AddEvent;

            hubConnection = new HubConnectionBuilder().WithUrl($"https://backboyserver.azurewebsites.net/chatHub").Build();
            //activeUsers = new List<string>();

            HandleConnections();
        }
        private void UpdateOnline(object sender, EventArgs e) {

            List<OnlineUser> temp = new List<OnlineUser>();
            foreach (var onlineUser in ObActiveUsers) {
                temp.Add(onlineUser);
            }
            ObActiveUsers = null;
            ObActiveUsers = new ObservableCollection<OnlineUser>();
            OnlineList.ItemsSource = ObActiveUsers;

            foreach (var entry in temp) {
                entry.setDateString();
                ObActiveUsers.Add(entry);
            }
        }
        private void HandleConnections() {
            hubConnection.On<string>("JoinChat", (user) => {
                PutOnScreen(null, user + " has connected");
            });

            hubConnection.On<string>("LeaveChat", (user) => {
                PutOnScreen(null, user + " has disconnected");
            });

            hubConnection.On<string, string, int, int, int >("ReceiveMessage", (user, message, r,g,b) => {
                System.Drawing.Color color = System.Drawing.Color.FromArgb(r,g,b);
                createMessageCell(user, message, color);
                ScrollWindow.ScrollToAsync(0, ChatArea.Height, true);
            });

            hubConnection.On<List<Message>>("ChatHistory", (msgList) => {
                if (!alreadyLoaded) {
                    foreach (Message msg in msgList) {
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(msg.R, msg.G, msg.B);
                        createMessageCell(msg.author, msg.message, color);
                    }
                    ScrollWindow.ScrollToAsync(0, ChatArea.Height, false);
                }
            });

            hubConnection.On<Message, int>("AddEvent", (evObj, index) => {
  
                noFriends.Text = "";
                Frame frame = createEventCell(evObj.author, evObj.message, evObj.time, Color.DarkBlue, evObj.R);
                twoWay.Add(evObj.R, frame);
                
                EventsArea.Children.Insert(index, frame);
            });

            hubConnection.On<List<Message>, bool, Message>("activeUserList", (activeUsers, added, who) => {
                if (added){
                    addToOnlineTab(activeUsers, who);
                }
                else {
                    removeFromOnlineTab(activeUsers,who);
                }
            });

            hubConnection.On<List<Message>>("EventsFromServer", (activeEvents) => {
                foreach (Message ev in activeEvents) {
                    Frame frame = createEventCell(ev.author, ev.message, ev.time, Color.DarkBlue, ev.R);
                    twoWay.Add(ev.R, frame);
                    EventsArea.Children.Add(frame);
                }
            });
            hubConnection.On<int>("RemoveEvent", (eventId) => {
                Frame frame = twoWay.GetFrame(eventId);
                EventsArea.Children.Remove(frame);
              
            });
            hubConnection.On<List<Message>>("EventHistory", (AllEvents) => {
                foreach (Message evObj in AllEvents) {
                    noFriends.Text = "";
                    Frame frame = createEventCell(evObj.author, evObj.message, evObj.time, Color.DarkBlue, evObj.R);
                    twoWay.Add(evObj.R, frame);

                    EventsArea.Children.Add(frame);
                }
            });

            hubConnection.On< List<string>, List<string>, int>("UpdateAttendingEvent", (attendList, declineList, eventId) => {
                if (!attending.ContainsKey(eventId)) {
                    attending.Add(eventId, attendList);
                }
                else {
                    attending[eventId] = attendList;
                }

                if (!declined.ContainsKey(eventId)) {
                    declined.Add(eventId, declineList);
                }
                else {
                    declined[eventId] = declineList;
                }
                try {
                    if(LastId == eventId)
                        LastVisitedEvent.updateAttedningStack(attendList, declineList);
                }
                catch { }
            });
        }
        private void addToOnlineTab(List<Message> active, Message user) {
            if (user.author.Equals(name)) {

                foreach (var person in active) {
                    OnlineUser adding = new OnlineUser() {name=person.author, comment="none", onlineSince = person.time};
                    adding.setDateString();
                    ObActiveUsers.Add(adding);
                }
                OnlineList.ItemsSource = ObActiveUsers;
            }
            else {
                OnlineUser adding = new OnlineUser() {name = user.author, comment = "none", onlineSince=user.time};
                adding.setDateString();
                ObActiveUsers.Add(adding);
            }
        }
        private void removeFromOnlineTab(List<Message> active, Message user){
            //DisplayAlert("Removed",  user+" logged out", "okay");
            OnlineUser removing = ObActiveUsers.FirstOrDefault(person => person.name.Equals(user.author));
            if (removing != null) {
                ObActiveUsers.Remove(removing);
            }
        }
        private string DateToString(DateTime time) {
            string minute = time.Minute < 10 ? minute = "0" + time.Minute.ToString() : time.Minute.ToString();
            return (time.Date.ToLongDateString()+" "+time.Hour+":"+minute);
        }

        async Task Connect() {
            try {
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("JoinChat", name);
                isConnected = true;
                ConnectsBtn.BackgroundColor = Color.FromHex("#EA596E");
                ConnectsBtn.Text = "Disconnect";
            }
            catch(Exception ex) {
                PutOnScreen(null, "Could not connect");
            }
        }

        private async void toggleBtn(object sender, EventArgs e) {
            if (isConnected) {await Disconnect();}
            else {await Connect();}
        }

        async Task Disconnect() {
            try {
                await hubConnection.InvokeAsync("LeaveChat", name);
                await hubConnection.StopAsync();
                isConnected = false;
                ConnectsBtn.BackgroundColor = Color.FromHex("#4BA2C7");
                ConnectsBtn.Text = "Connect";
            }
            catch {
                PutOnScreen(null, "Could not disconnect, that's too bad honestly");
            }
        }

        async Task SendMessage(string user, string message) {
            if (isConnected) {
                var stream = GetStreamFromFile("sent.mp3");
                player.Load(stream);

                for (int i = 0; i < 5; i++) { 
                    try {
                        await hubConnection.InvokeAsync("SendMessage", user, message, (int)cellColor.R, (int)cellColor.G, (int)cellColor.B);
                        break;
                    }
                    catch {
                        await hubConnection.StartAsync();
                    }
                }
            }
            else {
               await DisplayAlert("Ooops", "You cant send a message if you're not connected my guy!", "I'm dum dum");
            }
        }

        private async void Send_Clicked(object sender, EventArgs e) {
            string message = MessageEditor.Text;
            if (!string.IsNullOrEmpty(message)){
                await SendMessage(name, message);
            }
        }
        private void PutOnScreen(string name, string message) {
            Label LabMessage;
            if (name == null) {
               LabMessage = new Label { Text = message, FontSize = 20, TextColor = Color.White };
            }
            else {
                LabMessage = new Label { Text = name + ": " + message, FontSize = 20, TextColor = Color.Black };
            }
            LabMessage.HorizontalTextAlignment = TextAlignment.Center;
            MessageEditor.Text = "";
            ChatArea.Children.Add(LabMessage);
        }

        private async void AddEvent(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(popupRef.GetEditor().Text) && isConnected) {
                DateTime ba = new DateTime(popupRef.GetChosenDate().Date.Year, popupRef.GetChosenDate().Date.Month, popupRef.GetChosenDate().Date.Day, popupRef.GetChosenTime().Time.Hours, popupRef.GetChosenTime().Time.Minutes, 0);

                if (DateTime.Compare(ba, DateTime.Now) < 0) {
                    await DisplayAlert("Event warning", "Can't choose a date that has already passed my dude", "I'm the dum dum");
                }
                else{
                    await PopupNavigation.Instance.PopAsync(true);

                    for (int i = 0; i < 5; i++) {
                        try {
                            await hubConnection.InvokeAsync("AddEvent", name, popupRef.GetEditor().Text, ba);
                            break;
                        }
                        catch {
                            await hubConnection.StartAsync();
                        }
                    }
                }
            }
        }

        private void createMessageCell(string user, string message, System.Drawing.Color color) {
            
            Label MessageText = new Label() {FontSize=18, Text=message, HorizontalTextAlignment= user.Equals(name)? TextAlignment.End : TextAlignment.Start, TextColor = Color.White};
            Frame MessageFrame = new Frame() {
                BackgroundColor = color,
                Padding = 10,
                HasShadow = true,
                Margin = new Thickness(10, 10, 70, 10),
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Content = MessageText,
            };
            if (user.Equals(name)) {
                MessageFrame.HorizontalOptions = LayoutOptions.EndAndExpand;
                MessageFrame.Margin = new Thickness(70, 10, 10, 10);
            }
            MessageEditor.Text = "";
            ChatArea.Children.Add(MessageFrame);
        }
        private Frame createEventCell(string user, string title, DateTime time, System.Drawing.Color color, int id)
        {
            string dateString = DateToString(time);
            string shortString= String.Format("{0:m}", time);
            Label MessageText = new Label() { FontSize = 18, Text = title, HorizontalTextAlignment = TextAlignment.Center , TextColor= Color.White, Margin= new Thickness(0,24,0,32)};
            Label DateLab = new Label() { FontSize = 18, Text = shortString, HorizontalTextAlignment = TextAlignment.Center, TextColor= Color.White, Margin=new Thickness(0,16,0,16) };

            Color bodyCol = Color.FromHex("#5A5A5A");
            Color headerCOl = Color.FromHex("#4BA2C7");

            StackLayout header = new StackLayout() {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions= LayoutOptions.Fill,
            
                BackgroundColor = headerCOl,
            };
            header.Children.Add(DateLab);

            StackLayout body = new StackLayout() {BackgroundColor=bodyCol, HorizontalOptions=LayoutOptions.FillAndExpand, VerticalOptions= LayoutOptions.Start };
            body.Children.Add(MessageText);

            StackLayout sl = new StackLayout(){
                HorizontalOptions= LayoutOptions.FillAndExpand, 
                VerticalOptions= LayoutOptions.CenterAndExpand, 
            };
            sl.Children.Add(header);
            sl.Children.Add(body);


            Frame EventFrame = new Frame() {
                BorderColor = headerCOl,
                CornerRadius = 8,
                IsClippedToBounds=true,
                BackgroundColor = bodyCol,
                Padding = 0,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                Content = sl,

            };
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                LastId = id;

                List<string> atte = null;
                List<string> decl = null;

                if (attending.ContainsKey(LastId) && declined.ContainsKey(LastId)) {
                    atte = attending[LastId];
                    decl = declined[LastId];
                }
                EventPage ep = new EventPage(title, time, user, name, LastId, hubConnection, atte, decl);
                ep.getRemoveBtn().Clicked += RemoveEvent;
                ep.getAttendBtn().Clicked += AttendingBtn_Clicked;
                ep.getDeclineBtn().Clicked += DeclineBtn_Clicked;
                LastVisitedEvent = ep;
                Navigation.PushModalAsync(ep);

            };
            EventFrame.GestureRecognizers.Add(tapGestureRecognizer);

            Frame wrapppingFrame = new Frame() {HasShadow = false, CornerRadius = 6, Padding = 2,
                BackgroundColor = headerCOl, Content = EventFrame, Margin=new Thickness(0,0,0,32) }; 
            //EventsArea.Children.Add(EventFrame);
            return wrapppingFrame;
        }

        private async void RemoveEvent(object sender, EventArgs e) {
            await hubConnection.InvokeAsync("RemoveEvent", LastId);
            await Navigation.PopModalAsync();
        }

        private System.Drawing.Color GenColor() {
            Random rand = new Random();
            System.Drawing.Color col;

            do {
                col = System.Drawing.Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            }
            while ((col.R * 0.2126 + col.G * 0.7152 + col.B * 0.0722 > 255 / 2));

            return col;
        }
        Stream GetStreamFromFile(string filename) {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("AllianceDivisionApp." + filename);
            return stream;
        }

        private void RounBtn_Clicked(object sender, EventArgs e) {

            popupRef = new PopUpForm();
            popupRef.getAddBtn().Clicked += AddEvent;
            PopupNavigation.Instance.PushAsync(popupRef);
        }
        private async void AttendingBtn_Clicked(object sender, EventArgs e) {
            await hubConnection.InvokeAsync("AttendEvent", name, LastId);
        }

        private async void DeclineBtn_Clicked(object sender, EventArgs e) {
            await hubConnection.InvokeAsync("DeclineEvent", name, LastId);
        }

        private async void Logout_Clicked(object sender, EventArgs e) {
            Application.Current.Properties["logged"] = "";
            Application.Current.SavePropertiesAsync();
            try {
                await hubConnection.InvokeAsync("LeaveChat", name);
            }
            catch { }


                App.Current.MainPage = new MainPage();
        }

    }

    public class TwoWay {

        private Dictionary<int, Frame> idFrame;
        private Dictionary<Frame, int> FrameId;

        public TwoWay() {
            idFrame = new Dictionary<int, Frame>();
            FrameId = new Dictionary<Frame, int>();
        }
        public void Add(int id , Frame frame) {
            idFrame.Add(id, frame);
            FrameId.Add(frame, id);
        }

        public void Remove(int? id = null, Frame frame = null) {
            if (id == null) {
                id = FrameId[frame];
                FrameId.Remove(frame);
                idFrame.Remove((int)id);
            }
            else {
                frame = idFrame[(int)id];
                idFrame.Remove((int)id);
                FrameId.Remove(frame);
            }
        }
        public Frame GetFrame(int id) {
            return idFrame[id];
        }
        public int getId(Frame frame) {
            return FrameId[frame]; 
        }

    }

    public class OnlineUser { 
        public string name { get; set;}
        public string comment { get; set; }
        public DateTime onlineSince { get; set; }
        public string Since { get; set; }
        public void setDateString() {
            TimeSpan span = DateTime.Now.Subtract(onlineSince);
            double total = span.TotalMinutes;
            Since = total < 1 ? "Less than 1 minute" : (int)total + " minutes online";    
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
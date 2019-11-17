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

namespace AllianceDivisionApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage {
        ISimpleAudioPlayer player;

        bool alreadyLoaded = false;
        string name;
        HubConnection hubConnection;
        bool isConnected = false;
        System.Drawing.Color cellColor;

        
        public TabbedPage1(string name) {
            InitializeComponent();
            player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();


            Send.Clicked += Send_Clicked;
            this.name = name;
            Random rand = new Random();

            cellColor = GenColor(); 
                
            ConnectsBtn.Clicked += toggleBtn;
            AddBtn.Clicked += AddEvent;

            hubConnection = new HubConnectionBuilder().WithUrl($"https://backboy20191115101049.azurewebsites.net/chatHub").Build();
            HandleConnections();
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
                    alreadyLoaded = true;
                    ScrollWindow.ScrollToAsync(0, ChatArea.Height, false);
                }
            });

            hubConnection.On<Message>("AddEvent", (evObj) => {
                Label test = new Label() {Text=DateToString(evObj.time)};
                noFriends.Text = "";
                EventsArea.Children.Add(test);
            });
        }

        private string DateToString(DateTime time) {
            

            return (time.Date.ToLongDateString()+" "+time.Hour+":"+time.Minute);
        }

        async Task Connect() {
            try {
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("JoinChat", name, hubConnection.ConnectionId);
                isConnected = true;
                ConnectsBtn.BackgroundColor = Color.DarkRed;
                ConnectsBtn.Text = "Disconnect";
            }
            catch(Exception ex) {
                PutOnScreen(null, "Could not connect");
            }
        }

        private async void toggleBtn(object sender, EventArgs e) {
            if (isConnected) {
                await Disconnect();   
            }
            else {
                await Connect();
            }

        }
        async Task Disconnect() {
            try {
                await hubConnection.InvokeAsync("LeaveChat", name);
                await hubConnection.StopAsync();
                isConnected = false;
                ConnectsBtn.BackgroundColor = Color.RoyalBlue;
                ConnectsBtn.Text = "Connect";
            }
            catch {
                PutOnScreen(null, "Could not disconnect, that's too bad");

            }
        }
       

        async Task SendMessage(string user, string message) {
            if (isConnected) {
                var stream = GetStreamFromFile("sent.mp3");
                player.Load(stream);
                player.Play();

                await hubConnection.InvokeAsync("SendMessage", user, message, (int)cellColor.R, (int)cellColor.G, (int)cellColor.B);
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
               LabMessage = new Label { Text = message, FontSize = 20, TextColor = Color.Blue };
            }
            else {
                LabMessage = new Label { Text = name + ": " + message, FontSize = 20, TextColor = Color.Black };
            }
            LabMessage.HorizontalTextAlignment = TextAlignment.Center;
            MessageEditor.Text = "";
            ChatArea.Children.Add(LabMessage);

        }
        private async void AddEvent(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(EventEditor.Text) && isConnected) {
                DateTime ba = new DateTime(ChosenDate.Date.Year, ChosenDate.Date.Month, ChosenDate.Date.Day, ChosenTime.Time.Hours, ChosenTime.Time.Minutes, 0);

                await hubConnection.InvokeAsync("AddEvent", name, EventEditor.Text, ba);
            }
        
        }

        private void createMessageCell(string user, string message, System.Drawing.Color color) {
            
            Label MessageText = new Label() {FontSize=18, Text=message, HorizontalTextAlignment= user.Equals(name)? TextAlignment.End : TextAlignment.Start, TextColor = Color.Black};
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
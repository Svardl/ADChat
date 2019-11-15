using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllianceDivisionApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage {
        string name;
        HubConnection hubConnection;
        bool isConnected = false;

        public TabbedPage1(string name) {
            InitializeComponent();
            Send.Clicked += Send_Clicked;
            this.name = name;
            ConnectsBtn.Clicked += toggleBtn;

            hubConnection = new HubConnectionBuilder().WithUrl($"https://backboy20191115101049.azurewebsites.net/chatHub").Build();
            

            hubConnection.On<string>("JoinChat", (user) => {
                PutOnScreen(null, user+" has connected");
            });

            hubConnection.On<string>("LeaveChat", (user) => {
                PutOnScreen(null, user + " has disconnected");
            });

            hubConnection.On<string, string>("ReceiveMessage", (user, message) => {
                PutOnScreen(user, message);
            });

        }
        async Task Connect() {
            try {
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("JoinChat", name);
                isConnected = true;
            }
            catch(Exception ex) {
                PutOnScreen(name, "Could not connect");
            }
        }

        private async void toggleBtn(object sender, EventArgs e) {
            if (isConnected) {
                await Disconnect();
                ConnectsBtn.BackgroundColor = Color.RoyalBlue;
                ConnectsBtn.Text = "Connect";
            }
            else {
                await Connect();
                ConnectsBtn.BackgroundColor = Color.Red;
                ConnectsBtn.Text = "Disconnect";
            }

        }
        async Task Disconnect() {
            try {
                await hubConnection.InvokeAsync("LeaveChat", name);
                await hubConnection.StopAsync();
                isConnected = false;
            }
            catch {
                PutOnScreen(name, "Could not disconnect, that's too bad");

            }
        }
       

        async Task SendMessage(string user, string message) {
            if (isConnected)
                await hubConnection.InvokeAsync("SendMessage", user, message);
            else {
                DisplayAlert("Ooops", "You cant send a message if you're not connected my guy!", "I'm dum dum");
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
            MessageEditor.Text = "";
            ChatArea.Children.Add(LabMessage);

        }
        

    }
}
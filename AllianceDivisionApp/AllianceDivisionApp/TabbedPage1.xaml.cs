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

        public TabbedPage1(string name) {
            InitializeComponent();
            Send.Clicked += Send_Clicked;
            this.name = name;
            Connects.Clicked += Connect;

            hubConnection = new HubConnectionBuilder().WithUrl($"http://localhost:5001/chatHub").Build();
            

            hubConnection.On<string>("JoinChat", (user) => {
                PutOnScreen(user, "I connected");
            });

            hubConnection.On<string>("LeaveChat", (user) => {
            });

            hubConnection.On<string, string>("ReceiveMessage", (user, message) => {
                PutOnScreen(user, message);
            });

        }
        async void Connect(object sender, EventArgs e) {
            try {
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("JoinChat", name);
                PutOnScreen(name, "Connected");
            }
            catch {
                PutOnScreen(name, "Could not connect");
            }
        }

        async Task SendMessage(string user, string message) {
            await hubConnection.InvokeAsync("SendMessage", user, message);
        }

        private async void Send_Clicked(object sender, EventArgs e) {
            string message = MessageEditor.Text;
            if (!string.IsNullOrEmpty(message)){
                await SendMessage(name, message);
            }

        }
        private void PutOnScreen(string name, string message) {
            Label LabMessage = new Label { Text = name + ": " + message, FontSize = 22, TextColor = Color.Black };
                MessageEditor.Text = "";
                ChatArea.Children.Add(LabMessage);

        }
    }
}
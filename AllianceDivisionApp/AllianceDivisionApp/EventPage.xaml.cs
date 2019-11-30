using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllianceDivisionApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventPage : ContentPage {
        string title;
        DateTime time;
        string user;
        HubConnection hubConnection;
        public EventPage(string title, DateTime time, string eventCreator, string name, int id, HubConnection hubConnection, List<string> attedningList, List<string> declinedList ) {
            InitializeComponent();
            if (!name.Equals(eventCreator)) {
                RemoveBtn.IsVisible = false;
            }
            this.hubConnection = hubConnection;
            this.user = name;
            this.title = title;
            this.time = time;

            TitelLab.Text = title;

            DateLab.Text = time.ToString("dddd, dd MMMM yyyy");
            string hour = time.Hour < 10 ? "0" + time.Hour.ToString() : time.Hour.ToString();
            string minute = time.Minute < 10 ? "0" + time.Minute.ToString() : time.Minute.ToString();

            DateLab.Text += "   -   "+ hour+ ":" + minute;
            AdressLab.Text = "";

            foreach (string thing in attedningList) {
                TitleTimeStack.Children.Add(new Label() { Text = thing });
            }

        }
        public Button getRemoveBtn() {
            return RemoveBtn;
        }

        private async void AttendingBtn_Clicked(object sender, EventArgs e) {
            AttendingBtn.IsEnabled = false;
            DeclineBtn.IsEnabled = true;
            await hubConnection.InvokeAsync("AttendEvent", user, Id);
        }

        private async void DeclineBtn_Clicked(object sender, EventArgs e) {
            AttendingBtn.IsEnabled = true;
            DeclineBtn.IsEnabled = false;
            await hubConnection.InvokeAsync("DeclineEvent", user, Id);

        }
    }
}
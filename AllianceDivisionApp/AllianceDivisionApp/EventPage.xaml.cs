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
        string name;
        HubConnection hubConnection;
        int id;
        public EventPage(string title, DateTime time, string eventCreator, string name, int id, HubConnection hubConnection, List<string> attedningList, List<string> declinedList ) {
            InitializeComponent();
            if (!name.Equals(eventCreator)) {
                RemoveBtn.IsVisible = false;
            }
            //this.hubConnection = hubConnection;
            this.name = name;

            TitelLab.Text = title;

            DateLab.Text = time.ToString("dddd, dd MMMM yyyy");
            string hour = time.Hour < 10 ? "0" + time.Hour.ToString() : time.Hour.ToString();
            string minute = time.Minute < 10 ? "0" + time.Minute.ToString() : time.Minute.ToString();

            DateLab.Text += "   -   "+ hour+ ":" + minute;
            AdressLab.Text = "";

            updateAttedningStack(attedningList, declinedList);
        }

        public void updateAttedningStack(List<string> attedningList, List<string> declinedList) {

            AttedningStack.Children.Clear();

            if (attedningList != null) {
                foreach (string thing in attedningList) {
                    AttedningStack.Children.Add(new Label() { Text = thing, TextColor = Color.FromHex("#4DB6AC"), FontSize=20 });
                }
                if (attedningList.Contains(name)) {
                    AttendingBtn.IsEnabled = false;
                }
            }
            if (declinedList != null) {
                foreach (string declinedUser in declinedList) {
                    AttedningStack.Children.Add(new Label() { Text = declinedUser, TextColor = Color.FromHex("#EA596E"), FontSize = 20 });
                }
                if (declinedList.Contains(name)) {
                    DeclineBtn.IsEnabled = false;
                }
            }

        }
        public Button getRemoveBtn() {
            return RemoveBtn;
        }
        public Button getDeclineBtn() {
            return DeclineBtn;
        }
        public Button getAttendBtn() {
            return AttendingBtn;
        }

        private void AttendingBtn_Clicked_1(object sender, EventArgs e) {
            AttendingBtn.IsEnabled = false;
            DeclineBtn.IsEnabled = true;
        }

        private void DeclineBtn_Clicked_1(object sender, EventArgs e) {
            DeclineBtn.IsEnabled = false;
            AttendingBtn.IsEnabled = true;
        }
    }
}
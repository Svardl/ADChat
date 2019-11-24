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
        public EventPage(string title, DateTime time) {
            InitializeComponent();
            this.title = title;
            this.time = time;

            TitelLab.Text = title;

            DateLab.Text = time.ToString("dddd, dd MMMM yyyy");
            string hour = time.Hour < 10 ? "0" + time.Hour.ToString() : time.Hour.ToString();
            string minute = time.Minute < 10 ? "0" + time.Minute.ToString() : time.Minute.ToString();

            DateLab.Text += "   -   "+ hour+ ":" + minute;
            AdressLab.Text = "";
        }
    }
}
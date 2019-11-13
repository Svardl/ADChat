using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AllianceDivisionApp {
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage {

        Dictionary<string, string> namepass = new Dictionary<string, string> {
            {"Niclas", "Hello"},
            {"Julia", "Hello"},
            {"Kevin", "Hello"},
            {"Erik", "Hello"},
            {"Alex", "Hello"},
            {"Emil", "Hello"},
            {"Andreas", "Hello"},
            {"Harald", "Hello"},
            {"Isak", "Hello"},
            {"Ullberg", "Hello"},
        };

        public MainPage() {
            InitializeComponent();
        }

        private void Login_Clicked(object sender, EventArgs e) {
            string password = PasswordEditor.Text;
            string person = namePicker.SelectedItem as string;
            if (password == null) {
                password = "Hello";
            }

            if (person!=null || namepass[person].Equals(password) ) {
                DisplayAlert("Login Status", "Success!", "Okay");
                App.Current.MainPage = new TabbedPage1(person);
            }
            else {
                DisplayAlert("Login Status", "Failed!", "Okay");
            }

        }
    }
}

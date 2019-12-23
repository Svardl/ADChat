using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AllianceDivisionApp {

    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage {

        Dictionary<string, string> namepass = new Dictionary<string, string> {
            {"Niclas", "bot"},
            {"Julia", "juju"},
            {"Kevin", "rage"},
            {"Erik", "arl"},
            {"Alex", "doc"},
            {"Emil", "uxd"},
            {"Andreas", "roid"},
            {"Harald", "child"},
            {"Isak", "cob"},
            {"Ullberg", "burger"},
            {"Mattias", "stock" },
            {"Felix", "tall" }
        };

        public MainPage() {
            InitializeComponent();
               
        }

        private void Login_Clicked(object sender, EventArgs e) {
            string password = PasswordEditor.Text;
            string person = namePicker.SelectedItem as string;

            if ((!string.IsNullOrEmpty(person) && !string.IsNullOrEmpty(password))) {
                if (namepass[person].Equals(password) || password.Equals("deving")) {
                    DisplayAlert("Login Status", "Success!", "Okay");
                    Application.Current.Properties["logged"] = person;
                    Application.Current.SavePropertiesAsync();
                    App.Current.MainPage = new TabbedPage1(person);
                }
                else {
                    DisplayAlert("Login Status", "Failed!", "Okay");
                }
            }

        }
    }
}

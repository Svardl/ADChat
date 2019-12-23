using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllianceDivisionApp {
    public partial class App : Application {
        public App() {
            InitializeComponent();
            if (Application.Current.Properties.ContainsKey("logged") && !string.IsNullOrEmpty(Application.Current.Properties["logged"].ToString())) {
                MainPage = new TabbedPage1(Application.Current.Properties["logged"].ToString());
            }
            else
                MainPage = new MainPage();
        }

        protected override void OnStart() {
            // Handle when your app starts
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
        }

        protected override void OnResume() {
            // Handle when your app resumes
        }
    }
}

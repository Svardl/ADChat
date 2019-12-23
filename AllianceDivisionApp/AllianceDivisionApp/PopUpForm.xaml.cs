using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllianceDivisionApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopUpForm : PopupPage {
        public PopUpForm() {
            InitializeComponent();
        }

        public Entry GetEditor() { 
            return EventEditor; 
        }
        public DatePicker GetChosenDate() {
            return ChosenDate;
        }

        public TimePicker GetChosenTime() {
            return ChosenTime;
        }
        public Button getAddBtn() {
            return AddBtn;
        }


    }
}
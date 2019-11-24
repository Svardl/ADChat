using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using AllianceDivisionApp;
using AllianceDivisionApp.Droid;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;


[assembly: ExportRenderer(typeof(BorderlessPicker), typeof(droidPicker))]
namespace AllianceDivisionApp.Droid {
    [Obsolete]
    public class droidPicker : PickerRenderer {
        public static void Init() { }
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e) {
            base.OnElementChanged(e);
            if (Control != null) {
                Control.Background = new ColorDrawable(Android.Graphics.Color.Transparent);
            }
        
        }
    }
}